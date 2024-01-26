using PolyArchitect.TransferDefinitions;
using Godot;
namespace PolyArchitect.Editor
{
    using NodeID = int;
    using SceneID = string;
    public record NodeEditorState(SceneID SceneID, NodeID NodeID, Transform3D GlobalTransform, Transform3D LocalTransform) 
        : IDirectUpdateListener<NodeUpdateState, NodeEditorState> {

		public static Vector3 Convert(float3 val) 
			=> new(val.x, val.y, val.z);
		public static Quaternion Convert(float4 val) 
			=> new(val.w, val.x, val.y, val.z);
		public static Transform3D Convert(PosRotScale val){
            var rotBasis = new Basis(Convert(val.quatRot));
            var scaleBasis = Basis.FromScale(Convert(val.scale));
            return new Transform3D(rotBasis * scaleBasis, Convert(val.position));
        } 

        public static NodeEditorState FromUpdate(NodeUpdateState update) {
            return new(
                SceneID: update.SceneID,
                NodeID: update.NodeID,
                GlobalTransform: Convert(update.GlobalTransform),
                LocalTransform: Convert(update.LocalTransform)
            );
        }
    }
}