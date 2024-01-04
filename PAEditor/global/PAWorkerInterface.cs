using Microsoft.AspNetCore.SignalR.Client;
using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;


namespace PolyArchitect.Editor {

	using float3 = (float x, float y, float z);
	using float4 = (float w, float x, float y, float z);
	using GDArray = Godot.Collections.Array;
	using Request = (Task Task, string MethodName, Godot.Collections.Array Arguments);

	public partial class PAWorkerInterface : Node {
		private static HubConnection connection = null;
		private readonly static Dictionary<string, List<Callable>> msgNameToCallbacks = [];
		private static List<IDisposable> StaticSubscriptions = [];

		// pings are thread unsafe and can't handle > 1 pings
		// but this is good enough for now
        private static DateTime lastPingTime;

		private readonly static Queue<Request> requests = [];

		public override void _Ready()  {
			MakeConnection();
			StaticSubscriptions = BuildStaticSubscriptions();
		}
        public override void _Process(double delta)
        {
			ProcessRequests();
        }

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

		private async static void MakeConnection() {
			connection = new HubConnectionBuilder()
				.WithUrl("http://localhost:5036/debugTest").Build();
			await connection.StartAsync();
			if (connection.State == HubConnectionState.Connected) {
				Message("OnConnect");
			}
		}

		public static List<IDisposable> BuildStaticSubscriptions() =>
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
		];
		
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
		public static object ConvertArg(Variant vArg) {
			return vArg.Obj;
		}

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

	}

}