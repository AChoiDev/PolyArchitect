using System.Numerics;

namespace PolyArchitect.Core {
    public partial class CSGLink : INodeContent
    {
        public Matrix4x4 LocalTransform => throw new NotImplementedException();
        public readonly BooleanOperation operation;
    }
}