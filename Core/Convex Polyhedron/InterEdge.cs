using System.Collections.Generic;
using System.Linq;

namespace PolyArchitect.Core {

    // An edge of a convex polyhedron
    // Note that a point is defined by the intersection of 3 or more planes
    // 2 points form an edge if they share 2 or more planes
    public class InterEdge {
        public int pointIDA { get; private set; }
        public int pointIDB { get; private set; }
        public List<int> commonPlaneIDs { get; private set; }

        public InterEdge(int pointIDA, int pointIDB, Registry<InterPoint> reg) {
            this.pointIDA = pointIDA;
            this.pointIDB = pointIDB;
            commonPlaneIDs = CommonPlanes(reg.Get(pointIDA), reg.Get(pointIDB));
            if (commonPlaneIDs.Count < 2) {
                throw new System.Exception("Points do not form an edge");
            }
        }

        public static bool IsEdge(InterPoint pointA, InterPoint pointB) {
            return CommonPlanes(pointA, pointB).Count >= 2;
        }
        public static List<int> CommonPlanes(InterPoint pointA, InterPoint pointB) {
            var setA = pointA.planeIDs;
            var setB = pointB.planeIDs;
            return setA.Intersect(setB).ToList();
        }

    }
}