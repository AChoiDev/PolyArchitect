using Microsoft.AspNetCore.SignalR;
using PolyArchitect.Core;
using System.Numerics;
using PolyArchitect.TransferDefinitions;

namespace PolyArchitect.Worker {

    public class PersonalHub(AdoptionLifetime adoptionLifetime, PACoreService core) : Hub<IWorkerClient> {

        public static string CSGOpToStr(BooleanOperation op) {
            switch (op)
            {
                case BooleanOperation.Intersect: return "intersect";
                case BooleanOperation.Subtract: return "subtract";
                case BooleanOperation.Add: return "add";
                default: throw new Exception("operation invalid");
            }
        }
        public static BooleanOperation StrToCSGOp(string str) {
            switch (str)
            {
                case "intersect": return BooleanOperation.Intersect;
                case "subtract": return BooleanOperation.Subtract;
                case "add": return BooleanOperation.Add;
                default: throw new Exception("string was not a boolean operation");
            }
        }

        #region public Scene methods
        public async Task SaveScene(string sceneID, string path) {
            await Clients.All.SceneSaved(sceneID);
        }
        public async Task<string> CreateScene() {
            var strSceneID = core.Scenes.MakeScene().ToString();
            await Clients.All.SceneAvailable(strSceneID, true);
            return strSceneID;
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
    
        #region private MakeUpdate Methods
        private NodeUpdateState MakeNodeUpdate(string strSceneID, int nodeID) {
            var sceneID = int.Parse(strSceneID);
            var scene = core.Scenes.GetScene(sceneID);
            var globalTransform = scene.GetGlobalTransform(nodeID);
            var localTransform = scene.GetLocalTransform(nodeID);
            return new NodeUpdateState(strSceneID, nodeID, PosRotScale.From(globalTransform), PosRotScale.From(localTransform));
        }

        private CSGLinkUpdateState MakeCSGLinkUpdate(string strSceneID, int nodeID) {
            var nodeUpdateState = MakeNodeUpdate(strSceneID, nodeID);
            var sceneID = int.Parse(strSceneID);
            var scene = core.Scenes.GetScene(sceneID);

            // TODO: Use inheritance instead
            var nodeContent = scene.Get<INodeContent>(nodeID);

            BooleanOperation operation;
            if (nodeContent is CSGLink) {
                operation = ((CSGLink)nodeContent).operation;
            } else if (nodeContent is Brush) {
                operation = ((Brush)nodeContent).operation;
            } else {
                throw new Exception("content is not the correct type");
            }

            return new(nodeUpdateState, operation.ToString());
        }

        private BrushUpdateState MakeBrushUpdate(string strSceneID, int nodeID) {
            var csgLink = MakeCSGLinkUpdate(strSceneID, nodeID);

            return new BrushUpdateState(csgLink, 20);
        }
        private ProtoGlobalMeshUpdateState MakeGlobalMeshUpdate(string strSceneID) {
            var sceneID = int.Parse(strSceneID);
            var scene = core.Scenes.GetScene(sceneID);
            var globalMesh = scene.GenerateMesh();

            return new(
                Vertices: globalMesh.vertices.Select(float3.From).ToArray(), 
                Indices: globalMesh.indices.ToArray(), 
                Normals: globalMesh.normals.Select(float3.From).ToArray()
            );
        }
        #endregion

        public async Task<BrushUpdateState> CreateBrush(string strSceneID) {
            var sceneID = int.Parse(strSceneID);
            var scene = core.Scenes.GetScene(sceneID);
            var cubeShape = ConvexPolyhedron.Construct(BrushBuilders.MakeRectangularCuboid(Vector3.UnitY, Vector3.UnitZ).Item1).Item1;
            var brush = new Brush(cubeShape, BooleanOperation.Add);
            var brushID = scene.MakeNode(brush);

            var brushUpdate = MakeBrushUpdate(strSceneID, brushID);

            await Clients.All.NodeCreated(strSceneID, brushID);
            await Clients.All.BrushUpdate(brushUpdate);

            await Clients.All.ProtoGlobalMeshUpdate(MakeGlobalMeshUpdate(strSceneID));
            return brushUpdate;
        }

        public async Task<CSGLinkUpdateState> CreateCSGLink(string strSceneID) {
            var sceneID = int.Parse(strSceneID);
            var scene = core.Scenes.GetScene(sceneID);
            var nodeID = scene.MakeNode(new CSGLink(BooleanOperation.Add));

            var csgLinkUpdate = MakeCSGLinkUpdate(strSceneID, nodeID);

            await Clients.All.NodeCreated(strSceneID, nodeID);
            await Clients.All.CSGLinkUpdate(csgLinkUpdate);

            await Clients.All.ProtoGlobalMeshUpdate(MakeGlobalMeshUpdate(strSceneID));
            return csgLinkUpdate;
        }

        public async Task<CSGLinkUpdateState> SetCSGLinkOperation(string strSceneID, int nodeID, string operation) {
            var sceneID = int.Parse(strSceneID);
            var scene = core.Scenes.GetScene(sceneID);
            var nodeContent = scene.Get<INodeContent>(nodeID);
            // TODO: Use inheritance instead
            if (nodeContent is CSGLink) {
                ((CSGLink)nodeContent).operation = StrToCSGOp(operation);
            } else if (nodeContent is Brush) {
                ((Brush)nodeContent).operation = StrToCSGOp(operation);
            } else {
                throw new Exception("content is not the correct type");
            }

            var csgLinkUpdate = MakeCSGLinkUpdate(strSceneID, nodeID);

            await Clients.All.NodeCreated(strSceneID, nodeID);
            await Clients.All.CSGLinkUpdate(csgLinkUpdate);

            await Clients.All.ProtoGlobalMeshUpdate(MakeGlobalMeshUpdate(strSceneID));
            return csgLinkUpdate;
        }
        
        #region public General Node Methods
        public async Task<NodeUpdateState> SetNodeLocalTransform(string strSceneID, int nodeID, PosRotScale transform) {
            var sceneID = int.Parse(strSceneID);
            var scene = core.Scenes.GetScene(sceneID);
            transform.To(out Matrix4x4 matTransform);
            scene.SetLocalTransform(nodeID, matTransform);

            var updateState = MakeNodeUpdate(strSceneID, nodeID);

            await Clients.All.NodeUpdate(updateState);

            await Clients.All.ProtoGlobalMeshUpdate(MakeGlobalMeshUpdate(strSceneID));
            return updateState;
        }
        public async Task ParentNode(string strSceneID, int nodeID, int parentID, int childIdx) {
            var sceneID = int.Parse(strSceneID);
            var scene = core.Scenes.GetScene(sceneID);
            scene.SetParent(nodeID, parentID, childIdx);

            var hierarchy = "empty hierarchy";

            await Clients.All.HierarchyUpdate(strSceneID, hierarchy);
            await Clients.All.ProtoGlobalMeshUpdate(MakeGlobalMeshUpdate(strSceneID));
        }
        public async Task DestroyNode(string strSceneID, int nodeID) {
            await Clients.All.Destroyed(strSceneID, nodeID);
        }
        #endregion

        public async Task Ping() {
            await Clients.All.Pong(Context.ConnectionId);
        }

        public Task AdoptionCheckUp() {
            adoptionLifetime.CheckUp();
            return Task.CompletedTask;
        }

    }
}