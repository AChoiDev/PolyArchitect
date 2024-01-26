using PolyArchitect.TransferDefinitions;
using Godot;
namespace PolyArchitect.Editor {
    public record BrushEditorState(CSGLinkEditorState LinkState, int FaceCount) 
        : CSGLinkEditorState(LinkState),
        IIndirectUpdateListener<NodeUpdateState, BrushEditorState>,
        IDirectUpdateListener<BrushUpdateState, BrushEditorState>,
        IIndirectUpdateListener<CSGLinkUpdateState, BrushEditorState> {

        public static BrushEditorState FromUpdate(BrushUpdateState update) {
            return new(
                LinkState: FromUpdate(update.LinkState),
                FaceCount: update.FaceCount
            );
        }

        public new BrushEditorState WithUpdate(NodeUpdateState update) {
            return this with {LinkState = base.WithUpdate(update)};
        }

        public BrushEditorState WithUpdate(CSGLinkUpdateState update) {
            return this with {CSGOperation = update.Operation};
        }
    }
}