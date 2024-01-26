namespace PolyArchitect.TransferDefinitions {
    using NodeID = int;
    using SceneID = string;
    public interface INodeAddressable {
        SceneID SceneID {get;}
        NodeID NodeID {get;}
    }
}

