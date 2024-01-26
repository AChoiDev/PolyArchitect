namespace PolyArchitect.TransferDefinitions {

    using NodeID = int;
    using SceneID = string;
    public record NodeUpdateState(SceneID SceneID, NodeID NodeID, PosRotScale GlobalTransform, PosRotScale LocalTransform) : INodeAddressable;

    public record BrushUpdateState(CSGLinkUpdateState LinkState, int FaceCount) : CSGLinkUpdateState(LinkState);

    public record CSGLinkUpdateState(NodeUpdateState BaseState, string Operation) : NodeUpdateState(BaseState);

    public record class ProtoGlobalMeshUpdateState(
        float3[] Vertices, int[] Indices, float3[] Normals);
}

