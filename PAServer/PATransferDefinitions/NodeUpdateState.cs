namespace PolyArchitect.TransferDefinitions {
    using NodeID = int;
    using SceneID = string;
    public class NodeUpdateState(SceneID sceneID, NodeID nodeID, float3 position) : INodeAddressable {
        public SceneID SceneID => sceneID;
        public NodeID NodeID => nodeID;
        public (SceneID, NodeID) Address => (SceneID, NodeID);
        public float3 Position => position;
    }
}
