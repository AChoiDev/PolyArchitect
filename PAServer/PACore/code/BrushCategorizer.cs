using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PolyArchitect.Core {

    // A class for determining the sideness of a polygon with respoct to a convex polyhedron
    // TODO: rename this to side categorizer
    // merge this with another class
    public class BrushCategorizer {
        private readonly Dictionary<int, Plane> planeIDToPlane;

        public BrushCategorizer(ConvexPolyhedron polyhedron, Matrix4x4 transform) {
            planeIDToPlane = polyhedron.MakeTransformedPlanes(transform);
        }

        // returns point side
        // if point is aligned, return set of aligned planes
        public (PointSide, HashSet<int>) CategorizePoint(Vector3 queryPoint) {
            var EPSILON = 0.01f;

            var alignedSet = new HashSet<int>();
            var planeIndices = planeIDToPlane.Keys.ToList();
            var sides = planeIndices.Select((planeIndex) => planeIDToPlane[planeIndex].PointSide(queryPoint, EPSILON)).ToList();
            if (sides.Any((side) => side > 0)) {
                return (PointSide.OUTSIDE, alignedSet);
            } else if (sides.Any((side) => side == 0)) {
                alignedSet = planeIndices.FindAll(
                        (planeIndex) => planeIDToPlane[planeIndex].PointSide(queryPoint, EPSILON) == 0)
                        .ToHashSet();
                return (PointSide.ALIGNED, alignedSet);
            } else {
                return (PointSide.INSIDE, alignedSet);
            }

        }


        // we assume the polygon is either inside, outside, or aligned with brush
        public PolygonSide CategorizePolygon(ConvexPolygonGeometry polygon) {

            var vertices = polygon.FetchVertices();

            var result = vertices.Select((vertex) => CategorizePoint(vertex));
            var sideSet = result.Select((pair) => pair.Item1).ToHashSet();
            if (sideSet.Contains(PointSide.INSIDE)) {
                return PolygonSide.INSIDE;
            } else if (sideSet.Contains(PointSide.OUTSIDE)) {
                return PolygonSide.OUTSIDE;
            } else {

                var alignedSets = result.Select((pair) => pair.Item2).ToList();
                var intersectSet = alignedSets[0];

                foreach (var alignedSet in alignedSets) {
                    intersectSet.IntersectWith(alignedSet);
                }

                if (intersectSet.Count >= 1) {
                    var alignedPlane = planeIDToPlane[intersectSet.First()];
                    if (Vector3.Dot(polygon.Normal, alignedPlane.Normal) > 0) {
                        return PolygonSide.ALIGNED;
                    } else {
                        return PolygonSide.REVERSE_ALIGNED;
                    }
                } else {
                    return PolygonSide.INSIDE;
                }
            }
        }
    }
}