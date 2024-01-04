using Microsoft.AspNetCore.SignalR;

namespace PolyArchitect.Worker {
    using float3 = (float X, float Y, float Z);
    using float4 = (float W, float X, float Y, float Z);
    public class PersonalHub : Hub {
        // public override async Task OnConnectedAsync() {

        // }
#region Scene
        public async Task SaveScene(string sceneID, string path) {
            await Clients.All.SendAsync("SceneSaved", sceneID);
        }
        public async Task CreateScene() {
            var sceneID = "placeholder";
            await Clients.All.SendAsync("SceneAvailable", sceneID, true);
        }
        public async Task LoadScene(string path) {
            var sceneID = "placeholder";
            await Clients.All.SendAsync("SceneAvailable", sceneID, true);
        }
        public async Task UnloadScene(string sceneID) {
            await Clients.All.SendAsync("SceneAvailable", sceneID, false);
        }
#endregion
        
        public async Task CreateBrush(string sceneID) {
            int nodeID = -1;
            await Clients.All.SendAsync("NodeCreated", sceneID, nodeID);
        }
        public async Task SetTransform(string sceneID, int nodeID, float3 pos) {
            var transform = "placeholder";
            await Clients.All.SendAsync("TransformWasSet", nodeID, transform);
        }
        public async Task Parented(string sceneID, int nodeID, string parentID) {
            var hierarchy = "placeholder";
            await Clients.All.SendAsync("HierarchyUpdate", sceneID, hierarchy);
        }
        public async Task Destroy(string sceneID, int nodeID) {
            await Clients.All.SendAsync("Destroyed", sceneID, nodeID);
        }

        public async Task Ping() {
            await Clients.All.SendAsync("Pong", Context.ConnectionId);
        }
    }
}