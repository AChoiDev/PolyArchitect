using Microsoft.AspNetCore.SignalR.Client;
using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using PolyArchitect.TransferDefinitions;
using System.Diagnostics;

namespace PolyArchitect.Editor {

	using GDArray = Godot.Collections.Array;
	using Request = (Task Task, string MethodName, Godot.Collections.Array Arguments);

	public partial class PAWorkerInterface : Node {
		private static HubConnection connection = null;
		private readonly static Dictionary<string, List<Callable>> msgNameToCallbacks = [];
		private static List<IDisposable> StaticSubscriptions = [];

		// pings are thread unsafe and can't handle > 1 pings
		// but this is good enough for now
        private static DateTime lastPingTime;

		private static Task startPAWorkerTask;

		private readonly static Queue<Request> requests = [];

		public static void StaticSubscribe<C>(string methodName, System.Action<C> listener) {
			var handle = connection.On(methodName, listener);
			StaticSubscriptions.Add(handle);
		}

		public override void _Ready()  {
			startPAWorkerTask = StartPAWorker();
		}

		private static async Task StartPAWorker() {
			GD.Print("Starting PAWorker...");

			var runBuildBeforeStartingExecutable = true;
			if (runBuildBeforeStartingExecutable) {
				GDArray rawBuildOutputArray = [];
				var buildTask = Task.Run(() => {
					OS.Execute("dotnet", ["build", "../PAServer/PAWorker/PAWorker.csproj"], rawBuildOutputArray);
					var succeeded = ((string)rawBuildOutputArray[0]).Contains("Build succeeded");
					if (succeeded == false) {
						throw new Exception("Encountered build error with PAWorker");
					}
				});
				await buildTask;
				GD.Print("PAWorker csproj built with output:");
				GD.Print(rawBuildOutputArray[0]);
			}

			var pid = -1;
			var runTask = Task.Run(() => {
				var exePath = "../PAServer/PAWorker/bin/Debug/net8.0/PAWorker.exe";
				pid = OS.CreateProcess(exePath, [], true);
				if (pid == -1) {
					throw new Exception("Could not create worker process");
				}
			});
			await runTask;
			GD.Print($"PAWorker process started at pid: {pid}");
			var port = 5000;

			connection = new HubConnectionBuilder()
				.WithUrl($"http://localhost:{port}/debugTest").Build();
			await connection.StartAsync();
			GD.Print($"PAWorker connected, listening on port: {port}");

			StaticSubscriptions = BuildStaticSubscriptions();
			SceneEditorState.RegisterSubscriptions();
		}
        public override void _Process(double delta)
        {
			if (connection?.State == HubConnectionState.Connected) {
				Request("AdoptionCheckUp", []);
				ProcessRequests();
			}
			if (startPAWorkerTask.IsCompleted && startPAWorkerTask.IsCompletedSuccessfully == false) {
				GD.PrintErr("Could not start PAWorker, quitting application");
				GetTree().Quit();
				throw startPAWorkerTask.Exception;
			}
        }

		// Loops through each request in the queue, checking their status
		// dequeues if completed, requeues if in progress
		// throws exception if request encountered error
		public static void ProcessRequests() {
			var requestCount = requests.Count;
			for (int i = 0; i < requestCount; i++)
			{
				var request = requests.Dequeue();

				if (request.Task.Exception != null) {
					var argTypeNames = from arg in request.Arguments select arg.VariantType;
					var msg = $"Error occurred when requesting this method on the worker:\n{request.MethodName}({string.Join(", ", argTypeNames)})\n";
					var notatedException = new System.AggregateException(msg, request.Task.Exception);
					throw notatedException;
				}

				if (request.Task.IsCompleted) {
					request.Task.Dispose();
				} else {
					requests.Enqueue(request);
				}
				
			}
		}

		// global subscriptions to signalr messages
		// stores them for later disposal
		// TODO: dispose these on editor close
		private static List<IDisposable> BuildStaticSubscriptions() =>
		[
			connection.On<string, bool>("SceneAvailable", 
				(sceneID, availability) => {
					Message("SceneAvailable", sceneID, availability);
				}
			),
			connection.On<string>("SceneSaved", 
				(sceneID) => {
					Message("SceneSaved", sceneID);
				}
			),
			connection.On<string, int>("NodeCreated", 
				(sceneID, nodeID) => {
					Message("NodeCreated", sceneID, nodeID);
				}
			),
			connection.On<string>("Pong", 
				(id) => {
					var elapsed = DateTime.UtcNow - lastPingTime;
					var msg = $"Connection ID: {id}\nDuration: {elapsed.TotalMilliseconds} ms";
					Message("Pong", msg);
				}
			),
			connection.On<ProtoGlobalMeshUpdateState>(nameof(IWorkerClient.ProtoGlobalMeshUpdate), 
				(update) => {
					var meshState = ProtoGlobalMeshEditorState.FromUpdate(update);
					Message("GlobalMeshUpdated", meshState.MakeGodotMesh());
				}
			),
		];
		
		// general request method used by GDScript
		public static void Request(string requestName, GDArray vArguments) {
			// good-enough-for-now ping solution
			if (requestName == "Ping") {
				lastPingTime = DateTime.UtcNow;
			}

			var args = from vArg in vArguments select ConvertArg(vArg);
			var task = connection.InvokeCoreAsync(requestName, args.ToArray());
			requests.Enqueue((task, requestName, vArguments));
		}

		// TODO: Add conversions that map types like vector3 and quaternion4
		// to float3 and float4
		private static object ConvertArg(Variant vArg) {
			return vArg.Obj;
		}

		// basic observer pattern for handling updates obtained from the PAWorker
		public static void Subscribe(string msgName, Callable callable) {
			if (msgNameToCallbacks.TryGetValue(msgName, out var callbacks)) {
				callbacks.Add(callable);
			} else {
				msgNameToCallbacks.Add(msgName, [callable]);
			}
		}

		private static void Message(string name, params Variant[] args) {
			if (msgNameToCallbacks.TryGetValue(name, out var callbacks)) {
				callbacks.ForEach((cb) => cb.CallDeferred(args));
			}
		}


        public static string GetNodeDebugTxt(string sceneID, int nodeID) {
            var result = SceneEditorState.Get<object>(sceneID, nodeID).ToString();
            return result;
        }

		public static void RunCmdList(string cmdListName) {
			if (cmdListName == "apple") {
				Task task = MakeTestSceneApple();
			}
		}

		public static async Task MakeTestSceneApple() {
			var stopWatch = new Stopwatch();
			stopWatch.Start();

			var sceneID = await connection.InvokeAsync<string>("CreateScene");
			
			// var brushID = brushCreatedState.BaseState.NodeID;
			// await connection.InvokeAsync("ParentNode", sceneID, brushID, null, -1);
			var transformLink = new PosRotScale(new float3(0, 0.3f, 0), new float4(1, 0, 0, 0), scale: new float3(1, 1, 1));
			var transformOne = new PosRotScale(position: new(1, 0, 0), new float4(1, 0, 0, 0), scale: new(1, 3, 2));
			var transformTwo = new PosRotScale(position: new(0.5f, 0, 0), new float4(1, 0, 0, 0), scale: new(1, 3, 1));
			var transformThree = new PosRotScale(new float3(0, -0.1f, 0), new float4(1, 0, 0, 0), new float3(3, 2, 3));

			var CSGLinkID = (await connection.InvokeAsync<CSGLinkUpdateState>("CreateCSGLink", sceneID)).NodeID;
			await connection.InvokeAsync("SetCSGLinkOperation", sceneID, CSGLinkID, "add");
			await connection.InvokeAsync("SetNodeLocalTransform", sceneID, CSGLinkID, transformLink);

			var brushOneID = (await connection.InvokeAsync<BrushUpdateState>("CreateBrush", sceneID)).NodeID;
            await Task.Delay(3000);
			await connection.InvokeAsync("SetCSGLinkOperation", sceneID, brushOneID, "add");
			await connection.InvokeAsync("SetNodeLocalTransform", sceneID, brushOneID, transformOne);
			await connection.InvokeAsync("ParentNode", sceneID, brushOneID, CSGLinkID, -1);
            await Task.Delay(3000);

			var brushTwoID = (await connection.InvokeAsync<BrushUpdateState>("CreateBrush", sceneID)).NodeID;
            await Task.Delay(3000);
			await connection.InvokeAsync("SetNodeLocalTransform", sceneID, brushTwoID, transformTwo);
			await connection.InvokeAsync("ParentNode", sceneID, brushTwoID, CSGLinkID, -1);
            await Task.Delay(3000);
			await connection.InvokeAsync("SetCSGLinkOperation", sceneID, brushTwoID, "subtract");
            await Task.Delay(3000);

			var brushThreeID = (await connection.InvokeAsync<BrushUpdateState>("CreateBrush", sceneID)).NodeID;
			await connection.InvokeAsync("SetNodeLocalTransform", sceneID, brushThreeID, transformThree);
            await Task.Delay(3000);
			await connection.InvokeAsync("SetCSGLinkOperation", sceneID, brushThreeID, "intersect");

			stopWatch.Stop();
		}
	}

}