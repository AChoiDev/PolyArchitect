using System.Numerics;

namespace PolyArchitect.Core {
    public partial class CSGLink : INodeContent
    {
        public BooleanOperation operation;

        public CSGLink(BooleanOperation op) {
            operation = op;
        }
    }
}