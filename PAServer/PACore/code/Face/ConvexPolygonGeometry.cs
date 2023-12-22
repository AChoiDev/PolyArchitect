using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PolyArchitect.Core {

    // A convex polygon defined by a list of vertices
    // such that consecutive items of the list form an edge
    // also the start and end of the list are an edge
    public class ConvexPolygonGeometry {

        // Vertices in clockwise winding order
        public readonly List<int> LoopedVertexIDs;
        public List<Vector3> FetchVertices() => LoopedVertexIDs.Select((ID) => vertexManager.Get(ID)).ToList();

        private readonly IVertexManager vertexManager;

        public readonly Vector3 Normal;
        public readonly Vector3 AveragePos;

        public ConvexPolygonGeometry(List<int> loopedVertexIndices, IVertexManager vertexManager) {
            this.LoopedVertexIDs = loopedVertexIndices;
            this.vertexManager = vertexManager;

            // store common polygon properties
            var verts = FetchVertices();
            Normal = MathUtilities.GetCWNormal(verts[0], verts[1], verts[2]);
            AveragePos = verts.Aggregate(Vector3.Add) * (1f / verts.Count);
        }

        public ConvexPolygonGeometry MakeFlipped() {
            var reversedOrder = LoopedVertexIDs.Select((id) => id).Reverse().ToList();
            return new ConvexPolygonGeometry(reversedOrder, vertexManager);
        }

        public Plane GeneratePlane() {
            return new Plane(Normal, AveragePos);
        }

        public List<(int, int, int)> MakeTriangles() {
            if (LoopedVertexIDs.Count <= 2) {
                throw new System.Exception("INVALID POLYGON");
            }

            // TODO: Explain this method (eg preserves normal, splits convex shape into triangles)
            return Enumerable.Range(0, LoopedVertexIDs.Count - 2)
                .Select((i) => (LoopedVertexIDs[0], LoopedVertexIDs[i + 1], LoopedVertexIDs[i + 2]))
                .ToList();
        }

        public bool IsPointOutsidePolygonTube(Vector3 point) {
            var loop = FetchVertices();
            for (int i = 0; i < loop.Count; i++) {
                var l0 = loop[i];
                var l1 = loop[(i + 1) % loop.Count];
                var edgePlaneNormal = Vector3.Normalize(Vector3.Cross(l1 - l0, Normal));
                var edgePlanePoint = (l0 + l1) * 0.5f;
                var plane = new Plane(edgePlaneNormal, edgePlanePoint);
                var side = plane.PointSide(point);
                if (side > 0) {
                    return true;
                }
            }

            return false;
        }

        // TODO: make this code not suck, including "point outside polygon tube"
        public bool DoesCollide(ConvexPolygonGeometry otherPolygon) {
            var thisEdges = GraphUtilities.EdgesOfCircularList(LoopedVertexIDs);
            foreach (var edge in thisEdges) {
                var l0 = vertexManager.Get(edge.Item1);
                var l1 = vertexManager.Get(edge.Item2);
                var planeIntersection = Plane.LineIntersection(l0, l1, otherPolygon.AveragePos, otherPolygon.Normal);
                if (planeIntersection.HasValue) {
                    if (otherPolygon.IsPointOutsidePolygonTube(planeIntersection.Value) == false) {
                        return true;
                    }
                }
            }
            var otherEdges = GraphUtilities.EdgesOfCircularList(otherPolygon.LoopedVertexIDs);
            foreach (var edge in otherEdges) {
                var l0 = otherPolygon.vertexManager.Get(edge.Item1);
                var l1 = otherPolygon.vertexManager.Get(edge.Item2);
                var planeIntersection = Plane.LineIntersection(l0, l1, AveragePos, Normal);
                if (planeIntersection.HasValue) {
                    if (IsPointOutsidePolygonTube(planeIntersection.Value) == false) {
                        return true;
                    }
                }
            }


            return false;
        }


        public (List<int>, List<int>) PartitionPolygon(Plane slicingPlane) {
            var vertCount = LoopedVertexIDs.Count;
            var loopedVertices = FetchVertices();
            var backLoop = new List<int>();
            var frontLoop = new List<int>();
            for (var i = 0; i < vertCount; i++) {
                // start iteration at front index
                var loopIndex = i % vertCount;
                var vertexID = LoopedVertexIDs[loopIndex];
                var side = slicingPlane.PointSide(loopedVertices[loopIndex]);

                var nextLoopIndex = (loopIndex + 1) % LoopedVertexIDs.Count;
                var nextVertexID = LoopedVertexIDs[nextLoopIndex];
                var nextSide = slicingPlane.PointSide(loopedVertices[nextLoopIndex]);

                if (side >= 0) {
                    frontLoop.Add(vertexID);
                } else {
                    backLoop.Add(vertexID);
                }


                if ((side > 0 && nextSide < 0) || (side < 0 && nextSide > 0)) {
                    // found an edge that transitions between a back and a front node
                    // must create point that intersects with plane and add to both lists
                    var newVertexID = vertexManager.Add(slicingPlane.LineIntersectionClamped(vertexManager.Get(vertexID), vertexManager.Get(nextVertexID)));
                    frontLoop.Add(newVertexID);
                    backLoop.Add(newVertexID);
                }
                if ((side == 0 && nextSide < 0) || (side < 0 && nextSide == 0)) {
                    // found an edge between a back node and a mid node
                    // must add the mid node into the backlist
                    if (side == 0) {
                        backLoop.Add(vertexID);
                    } else {
                        backLoop.Add(nextVertexID);
                    }
                }
            }

            return (backLoop, frontLoop);
        }

        // TODO: Simplify this, including the partitioner
        public List<ConvexPolygonGeometry> Slice(Plane slicingPlane) {
            var (backLoop, frontLoop) = PartitionPolygon(slicingPlane);

            var slicedPolygons = new List<ConvexPolygonGeometry>();
            if (backLoop.Count >= 3) {
                slicedPolygons.Add(new ConvexPolygonGeometry(backLoop, vertexManager));
            }
            if (frontLoop.Count >= 3) {
                slicedPolygons.Add(new ConvexPolygonGeometry(frontLoop, vertexManager));
            }

            return slicedPolygons;
        }

        public void PenDebugDraw() {
            var vertices = FetchVertices();
            for (int i = 0; i < vertices.Count; i++)
            {
                var j = (i + 1) % vertices.Count;
                PenDebug.DrawLine(vertices[i], vertices[j]);
            }
        }
    }

}