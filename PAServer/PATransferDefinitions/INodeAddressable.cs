namespace PolyArchitect.TransferDefinitions {
    using NodeID = int;
    using SceneID = string;
    public interface INodeAddressable {
        public (SceneID, NodeID) Address {get;}
    }
}

