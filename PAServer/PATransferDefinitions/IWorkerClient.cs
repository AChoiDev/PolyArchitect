namespace PolyArchitect.TransferDefinitions {
    using SceneID = string;
    using NodeID = int;
    public interface IWorkerClient {
        Task SceneSaved(SceneID sceneID);
        Task SceneAvailable(SceneID sceneID, bool availability);
        Task NodeUpdate(NodeUpdateState update);
        Task BrushUpdate(BrushUpdateState update);
        Task CSGLinkUpdate(CSGLinkUpdateState update);

        // TODO: remove node created
        Task NodeCreated(SceneID sceneID, NodeID nodeID);

        Task HierarchyUpdate(SceneID sceneID, string hierarchy);
        Task Destroyed(SceneID sceneID, NodeID nodeID);
        Task Pong(string connectionID);
        Task ProtoGlobalMeshUpdate(ProtoGlobalMeshUpdateState update);
    }
}
