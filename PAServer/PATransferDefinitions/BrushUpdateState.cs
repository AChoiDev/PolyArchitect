namespace PolyArchitect.TransferDefinitions {
    using NodeID = int;
    using SceneID = string;
    public class BrushUpdateState(NodeUpdateState baseState, int faceCount) : INodeAddressable {
        public int FaceCount => faceCount;
        public NodeUpdateState BaseState => baseState;
        public (SceneID, NodeID) Address => baseState.Address;
    }
}

