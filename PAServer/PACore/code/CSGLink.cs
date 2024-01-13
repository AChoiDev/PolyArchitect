using System.Numerics;

namespace PolyArchitect.Core {
    public partial class CSGLink : INodeContent
    {
        public readonly BooleanOperation operation;

        public CSGLink(BooleanOperation op) {
            operation = op;
        }
    }
}