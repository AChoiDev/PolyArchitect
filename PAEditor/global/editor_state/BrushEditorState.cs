using PolyArchitect.TransferDefinitions;
using Godot;
namespace PolyArchitect.Editor {
    public class BrushEditorState(NodeEditorState baseState, int faceCount) 
        : IIndirectUpdateListener<NodeUpdateState, BrushEditorState>,
        IDirectUpdateListener<BrushUpdateState, BrushEditorState> {

        public int FaceCount => faceCount;
        public NodeEditorState BaseState => baseState;

        // Listener implementations
        public static BrushEditorState Update(BrushUpdateState update) => new(
            baseState: NodeEditorState.Update(update.BaseState),
            faceCount: update.FaceCount
        );

        public BrushEditorState Update(NodeUpdateState update) {
            baseState = NodeEditorState.Update(update);
            return this;
        }
    }
}