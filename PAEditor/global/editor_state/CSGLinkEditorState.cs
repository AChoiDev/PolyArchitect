using PolyArchitect.TransferDefinitions;
using Godot;
namespace PolyArchitect.Editor {
    public record CSGLinkEditorState(NodeEditorState BaseState, string CSGOperation)
        : IIndirectUpdateListener<NodeUpdateState, CSGLinkEditorState>,
        IDirectUpdateListener<CSGLinkUpdateState, CSGLinkEditorState>
    {
        public static CSGLinkEditorState FromUpdate(CSGLinkUpdateState update) {
            return new(
                BaseState: NodeEditorState.FromUpdate(update.BaseState),
                CSGOperation: update.Operation
            );
        }

        public CSGLinkEditorState WithUpdate(NodeUpdateState update) {
            return this with {BaseState = NodeEditorState.FromUpdate(update)};
        }
    }
}