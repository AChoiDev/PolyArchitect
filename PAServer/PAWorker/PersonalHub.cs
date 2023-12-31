using Microsoft.AspNetCore.SignalR;

namespace PolyArchitect.Worker {
    public class PersonalHub : Hub {
        public override async Task OnConnectedAsync() {

            await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined!");
        }

        public async Task Ping() {
            await Clients.All.SendAsync("Pong", Context.ConnectionId);
        }
    }
}