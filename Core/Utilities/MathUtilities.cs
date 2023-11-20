using System.Numerics;

namespace PolyArchitect.Core {

    // TODO: move more spatial math stuff here, like plane intersection
    public static class MathUtilities {

        public static Vector3 GetCWNormal(Vector3 point1, Vector3 point2, Vector3 point3) {
            return Vector3.Normalize(Vector3.Cross(point2 - point1, point3 - point1));
        }
    }
}