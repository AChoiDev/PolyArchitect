using PolyArchitect.TransferDefinitions;
using Godot;
namespace PolyArchitect.Editor
{
    using NodeID = int;
    using SceneID = string;
    public class NodeEditorState(SceneID sceneID, NodeID nodeID, Vector3 position) 
        : IDirectUpdateListener<NodeUpdateState, NodeEditorState> {

		public static Vector3 Convert(float3 val) 
			=> new(val.x, val.y, val.z);
		public static Quaternion Convert(float4 val) 
			=> new(val.w, val.x, val.y, val.z);

        public SceneID SceneID => sceneID;
        public NodeID NodeID => nodeID;
        public Vector3 Position => position;

        public static NodeEditorState Update(NodeUpdateState update) => new(
            sceneID: update.SceneID,
            nodeID: update.NodeID,
            position: Convert(update.Position)
        );
    }
}