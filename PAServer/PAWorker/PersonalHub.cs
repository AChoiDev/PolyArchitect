using Microsoft.AspNetCore.SignalR;
using PolyArchitect.Core;
using System.Numerics;

namespace PolyArchitect.Worker {
    using float3 = (float X, float Y, float Z);
    using float4 = (float W, float X, float Y, float Z);
    public class PersonalHub : Hub {
        private readonly SceneRegistry sceneReg = new();
        // public override async Task OnConnectedAsync() {

        // }
#region Scene
        public async Task SaveScene(string sceneID, string path) {
            await Clients.All.SendAsync("SceneSaved", sceneID);
        }
        public async Task CreateScene() {
            var strSceneID = sceneReg.MakeScene().ToString();
            await Clients.All.SendAsync("SceneAvailable", strSceneID, true);
        }
        public async Task LoadScene(string path) {
            var sceneID = "placeholder";
            await Clients.All.SendAsync("SceneAvailable", sceneID, true);
        }
        public async Task UnloadScene(string strSceneID) {
            var sceneID = Guid.Parse(strSceneID);
            sceneReg.DeleteScene(sceneID);

            await Clients.All.SendAsync("SceneAvailable", strSceneID, false);
        }
#endregion
        
        public async Task CreateBrush(string strSceneID) {
            var sceneID = Guid.Parse(strSceneID);
            var scene = sceneReg.GetScene(sceneID);
            var cubeShape = ConvexPolyhedron.Construct(BrushBuilders.MakeRectangularCuboid(Vector3.UnitY, Vector3.UnitZ).Item1).Item1;
            var brush = new Brush(cubeShape, BooleanOperation.Add);
            var brushID = scene.MakeNode(brush);
            await Clients.All.SendAsync("NodeCreated", sceneID, brushID);
        }
        public async Task SetTransform(string strSceneID, int nodeID, float3 pos) {
            var transform = "placeholder";
            await Clients.All.SendAsync("TransformWasSet", nodeID, transform);
        }
        public async Task Parent(string strSceneID, int nodeID, int parentID, int childIdx) {
            var hierarchy = "placeholder";
            await Clients.All.SendAsync("HierarchyUpdate", strSceneID, hierarchy);
        }
        public async Task Destroy(string strSceneID, int nodeID) {
            await Clients.All.SendAsync("Destroyed", strSceneID, nodeID);
        }

        public async Task Ping() {
            await Clients.All.SendAsync("Pong", Context.ConnectionId);
        }
    }
}