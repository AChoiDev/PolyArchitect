using System.Numerics;

namespace PolyArchitect.Core {
    public interface INodeContent {
        // TODO: define some way for a node to only allow select types of children
        public virtual void OnGlobalTransformChange(Matrix4x4 transform) {}
    }
}
