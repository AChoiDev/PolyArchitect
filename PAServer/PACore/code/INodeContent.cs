using System.Numerics;

namespace PolyArchitect.Core {
    public interface INodeContent {
        // public Matrix4x4 LocalTransform {get;}
        // public AxisAlignedBoundingBox AABB(Matrix4x4 worldTransform);
        public virtual void OnTransformChange(Matrix4x4 transform) {}
    }
}
