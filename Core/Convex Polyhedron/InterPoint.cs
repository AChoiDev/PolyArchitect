using System.Linq;
using System.Numerics;
using System;
using System.Collections.Generic;

namespace PolyArchitect.Core {

// A 3D point formed from the intersection of 3 or more planes
// Stores the IDs of the intersection planes
public class InterPoint {
    public HashSet<int> planeIDs {get; private set;}
    public Vector3 SpatialPoint {get; private set;}

    public InterPoint(int idOne, int idTwo, int idThree, Registry<Plane> reg) {
        planeIDs = new(){idOne, idTwo, idThree};
        SpatialPoint = PointFromPlaneIntersection(reg, idOne, idTwo, idThree).Value;
    }

    public void AddPlane(int id) {
        planeIDs.Add(id);
    }
    public void RemovePlane(int id) {
        planeIDs.Remove(id);
    }



    public override string ToString()
    {
        return "(" + string.Join(", ", planeIDs) + ")";
    }

    public static Vector3? PointFromPlaneIntersection(Registry<Plane> planes, int planeIDA, int planeIDB, int planeIDC) {
        var planeA = planes.Get(planeIDA);
        var planeB = planes.Get(planeIDB);
        var planeC = planes.Get(planeIDC);
        return PointFromPlaneIntersection(planeA.Normal, planeA.GetPlanePoint(), planeB.Normal, planeB.GetPlanePoint(), planeC.Normal, planeC.GetPlanePoint());
    }

    // TODO: cleanup this code
    public static Vector3? PointFromPlaneIntersection(Vector3 planeANorm, Vector3 planeAPos, Vector3 planeBNorm, Vector3 planeBPos, Vector3 planeCNorm, Vector3 planeCPos) {
        // TODO: Make this plane dist based
        var EPSILON = 0.000001f;

        var edgeDir = Vector3.Cross(planeANorm, planeBNorm);
        var tangent = Vector3.Cross(planeANorm, edgeDir);
        
        var kDivisor = Vector3.Dot(tangent, planeBNorm);

        if (Math.Abs(kDivisor) < EPSILON) {
            return null;
        }

        var k = Vector3.Dot(planeBPos - planeAPos, planeBNorm) / kDivisor;

        var edgePos = planeAPos + tangent * k;

        var gDivisor = Vector3.Dot(edgeDir, planeCNorm);
        if (Math.Abs(gDivisor) < EPSILON) {
            return null;
        }

        var g = Vector3.Dot(planeCPos - edgePos, planeCNorm) / gDivisor;

        var pointPos = edgePos + g * edgeDir;

        return pointPos;
    }

}
}