using Microsoft.AspNetCore.SignalR;
using PolyArchitect.Core;
using System.Numerics;
using PolyArchitect.TransferDefinitions;

namespace PolyArchitect.Worker {

    public class PersonalHub(AdoptionLifetime adoptionLifetime, PACoreService core) : Hub<IWorkerClient> {

        #region Scene
        public async Task SaveScene(string sceneID, string path) {
            await Clients.All.SceneSaved(sceneID);
        }
        public async Task CreateScene() {
            var strSceneID = core.Scenes.MakeScene().ToString();
            await Clients.All.SceneAvailable(strSceneID, true);
        }
        public async Task LoadScene(string path) {
            var sceneID = "placeholder";
            await Clients.All.SceneAvailable(sceneID, true);
        }
        public async Task UnloadScene(string strSceneID) {
            var sceneID = Guid.Parse(strSceneID);
            // sceneReg.DeleteScene(sceneID);

            await Clients.All.SceneAvailable(strSceneID, false);
        }
#endregion
    
        public async Task CreateBrush(string strSceneID) {
            var sceneID = int.Parse(strSceneID);
            var scene = core.Scenes.GetScene(sceneID);
            var cubeShape = ConvexPolyhedron.Construct(BrushBuilders.MakeRectangularCuboid(Vector3.UnitY, Vector3.UnitZ).Item1).Item1;
            var brush = new Brush(cubeShape, BooleanOperation.Add);
            var brushID = scene.MakeNode(brush);

            var nodeState = new NodeUpdateState(strSceneID, brushID, new(4f, 5f, 6f));
            var updateState = new BrushUpdateState(nodeState, 5);

            await Clients.All.NodeCreated(strSceneID, brushID);
            await Clients.All.BrushUpdate(updateState);
        }
        public async Task SetTransform(string strSceneID, int nodeID, float3 pos) {
            var nodeState = new NodeUpdateState("fakeSceneID", 6, new(4f, 5f, 6f));
            await Clients.All.NodeUpdate(nodeState);
        }
        public async Task Parent(string strSceneID, int nodeID, int parentID, int childIdx) {
            var hierarchy = "placeholder";
            await Clients.All.HierarchyUpdate(strSceneID, hierarchy);
        }
        public async Task Destroy(string strSceneID, int nodeID) {
            await Clients.All.Destroyed(strSceneID, nodeID);
        }

        public async Task Ping() {
            await Clients.All.Pong(Context.ConnectionId);
        }

        public Task AdoptionCheckUp() {
            adoptionLifetime.CheckUp();
            return Task.CompletedTask;
        }
    }
}