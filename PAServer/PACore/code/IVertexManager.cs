using System.Numerics;

namespace PolyArchitect.Core {
    public interface IVertexManager {
        public int Add(Vector3 point);
        public Vector3 Get(int ID);
    }
}