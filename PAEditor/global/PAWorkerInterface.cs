using Microsoft.AspNetCore.SignalR.Client;
using Godot;
using System.Collections.Generic;

namespace PolyArchitect.Editor {
	public partial class PAWorkerInterface : Node {
		private static HubConnection connection = null;
		private readonly static Dictionary<string, List<Callable>> msgNameToCallbacks = [];

		public override void _Ready()  {
			MakeConnection();
		}

		private async static void MakeConnection() {
			connection = new HubConnectionBuilder()
				.WithUrl("http://localhost:5036/debugTest").Build();
			await connection.StartAsync();
			if (connection.State == HubConnectionState.Connected) {
				Message("OnConnect");
			}
		}

		private static void Message(string name, params Variant[] args) {
			if (msgNameToCallbacks.TryGetValue(name, out var callbacks)) {
				callbacks.ForEach((cb) => cb.Call(args));
			}
		}

		public async static void Ping() {
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			var pongResult = "";
			var handler = connection.On<string>("Pong", (str) => {
				pongResult = str;
				stopwatch.Stop();
			});
			await connection.InvokeAsync("Ping");
			handler.Dispose();

			var pingInfo = $"Connection ID: {pongResult}\nDuration: {stopwatch.Elapsed.TotalMilliseconds} ms";
			Message("Pong", pingInfo);
		}

		public static void Subscribe(string msgName, Callable callable) {
			if (msgNameToCallbacks.TryGetValue(msgName, out var callbacks)) {
				callbacks.Add(callable);
			} else {
				msgNameToCallbacks.Add(msgName, [callable]);
			}
		}

	}

}