using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEditor;

namespace PolyArchitect.Core {

    // A convex polyhedron defined by a set of intersecting planes
    // vertices, edges, and polygons are inferred by a clipping algorithm
    // TODO: Support plane removal for planes that become redundant
    public partial class ConvexPolyhedron {


        private readonly Registry<Plane> planeRegistry = new();
        private readonly Registry<InterEdge> edgeRegistry = new();
        private readonly Registry<InterPoint> pointRegistry = new();
        public const float GLOBAL_LENGTH_LIMIT = 2000f;

        private ConvexPolyhedron() {
            var R = (GLOBAL_LENGTH_LIMIT + 13.4204242f) * 0.5f;
            var pIDXPos = planeRegistry.Add(new Plane(Vector3.UnitX, R));
            var pIDXNeg = planeRegistry.Add(new Plane(-Vector3.UnitX, R));
            var pIDYPos = planeRegistry.Add(new Plane(Vector3.UnitY, R));
            var pIDYNeg = planeRegistry.Add(new Plane(-Vector3.UnitY, R));
            var pIDZPos = planeRegistry.Add(new Plane(Vector3.UnitZ, R));
            var pIDZNeg = planeRegistry.Add(new Plane(-Vector3.UnitZ, R));

            var pointList = new List<InterPoint>(){
            new (pIDXPos, pIDYPos, pIDZPos, planeRegistry),
            new (pIDXPos, pIDYPos, pIDZNeg, planeRegistry),
            new (pIDXPos, pIDYNeg, pIDZPos, planeRegistry),
            new (pIDXPos, pIDYNeg, pIDZNeg, planeRegistry),
            new (pIDXNeg, pIDYPos, pIDZPos, planeRegistry),
            new (pIDXNeg, pIDYPos, pIDZNeg, planeRegistry),
            new (pIDXNeg, pIDYNeg, pIDZPos, planeRegistry),
            new (pIDXNeg, pIDYNeg, pIDZNeg, planeRegistry)
        };

            pointList.Select((point) => pointRegistry.Add(point)).ToList();
            var pointCount = pointRegistry.Count;
            var pointIDs = pointRegistry.GetIDs().ToList();

            for (var i = 0; i < pointCount - 1; i++) {
                for (var j = i + 1; j < pointCount; j++) {
                    var idA = pointIDs[i];
                    var idB = pointIDs[j];
                    if (InterEdge.IsEdge(pointRegistry.Get(idA), pointRegistry.Get(idB))) {
                        edgeRegistry.Add(new InterEdge(idA, idB, pointRegistry));
                    }
                }
            }
        }

        public static (ConvexPolyhedron, List<int>) Construct(List<Plane> planeParams) {
            var shape = new ConvexPolyhedron();
            var clipPlaneIDs = new List<int>();
            foreach (var plane in planeParams) {
                var clipPlaneID = shape.Clip(plane);
                clipPlaneIDs.Add(clipPlaneID);
            }
            return (shape, clipPlaneIDs);
        }

        public int Clip(Plane clipPlane) {
            var clipPlaneID = planeRegistry.Add(clipPlane);

            var edgeIDs = edgeRegistry.GetIDs();
            var pointsInPlaneID = new HashSet<int>();
            foreach (var edgeID in edgeIDs) {
                ClipEdge(edgeID, clipPlaneID, pointsInPlaneID);
            }

            InferPlaneEdges(clipPlaneID, pointsInPlaneID)
            .ToList()
            .ForEach((edge) => edgeRegistry.Add(edge));

            CullUnusedPoints();

            return clipPlaneID;
        }

        public HashSet<Vector3> GetTransformedVertices(Matrix4x4 transform) {
            return pointRegistry.GetValues().Select(
                    (point) => Vector3.Transform(point.SpatialPoint, transform)
                ).ToHashSet();
        }

        public Dictionary<int, Plane> MakeTransformedPlanes(Matrix4x4 transform) {
            var modDic = new Dictionary<int, Plane>();
            var planeIDs = planeRegistry.GetIDs();
            foreach (var planeID in planeIDs) {
                modDic[planeID] = planeRegistry.Get(planeID).Transform(transform);
            }

            return modDic;
        }

        public Vector3 GetPlaneNormal(int planeID) {
            return planeRegistry.Get(planeID).Normal;
        }

        private void ClipEdge(int edgeID, int clipPlaneID, HashSet<int> intersectedPointIDs) {
            var epsilon = 0.01f;
            var clipPlane = planeRegistry.Get(clipPlaneID);

            var edge = edgeRegistry.Get(edgeID);
            var pointA = pointRegistry.Get(edge.pointIDA);
            var pointB = pointRegistry.Get(edge.pointIDB);

            var sideA = clipPlane.PointSide(pointA.SpatialPoint, epsilon);
            var sideB = clipPlane.PointSide(pointB.SpatialPoint, epsilon);
            if (sideA == 0f) {
                pointRegistry.Get(edge.pointIDA).AddPlane(clipPlaneID);
                intersectedPointIDs.Add(edge.pointIDA);
            }
            if (sideB == 0f) {
                pointRegistry.Get(edge.pointIDB).AddPlane(clipPlaneID);
                intersectedPointIDs.Add(edge.pointIDB);
            }

            // intersection case
            if ((sideA < 0f && sideB > 0f)
                || (sideA > 0f && sideB < 0f)) {
                var backPointID = sideA < 0f ? edge.pointIDA : edge.pointIDB;
                var interPoint = new InterPoint(edge.commonPlaneIDs[0], edge.commonPlaneIDs[1], clipPlaneID, planeRegistry);
                var interPointID = pointRegistry.Add(interPoint);
                edgeRegistry.Add(new InterEdge(interPointID, backPointID, pointRegistry));
                intersectedPointIDs.Add(interPointID);
            }
            if (sideA > 0f || sideB > 0f) {
                // this includes intersection cases
                edgeRegistry.Remove(edgeID);
            }
        }

        private HashSet<InterEdge> InferPlaneEdges(int inferencePlaneID, HashSet<int> intersectedPointIDs) {
            // create data structure to find new edges
            var planeIDToPointList = new Dictionary<int, List<int>>();
            var planeIDs = planeRegistry.GetIDs();
            foreach (var planeID in planeIDs) {
                planeIDToPointList.Add(planeID, new());
            }

            foreach (var intersectedPointID in intersectedPointIDs) {
                var intersectedPoint = pointRegistry.Get(intersectedPointID);
                foreach (var planeID in intersectedPoint.planeIDs) {
                    if (planeID != inferencePlaneID) {
                        planeIDToPointList[planeID].Add(intersectedPointID);
                    }
                }
            }

            // infer the edges in the plane
            var inferredEdges = new HashSet<InterEdge>();
            foreach (var pointList in planeIDToPointList.Values) {
                if (pointList.Count == 2) {
                    inferredEdges.Add(new InterEdge(pointList[0], pointList[1], pointRegistry));
                }
                if (pointList.Count > 2) {
                    throw new System.Exception("Invalid clip plane");
                }
            }

            return inferredEdges;
        }

        // every point not in an edge is removed
        private void CullUnusedPoints() {
            var validPointIDs = new HashSet<int>();
            var allPointIDs = pointRegistry.GetIDs();
            var allEdges = edgeRegistry.GetValues();
            foreach (var edge in allEdges) {
                validPointIDs.Add(edge.pointIDA);
                validPointIDs.Add(edge.pointIDB);
            }

            var invalidPointIDs = allPointIDs.Except(validPointIDs).ToHashSet();
            foreach (var invalidPointID in invalidPointIDs) {
                pointRegistry.Remove(invalidPointID);
            }
        }



        private void draw() {
            var edges = edgeRegistry.GetValues();
            var points = pointRegistry.GetValues();
            foreach (var edge in edges) {
                var pointA = pointRegistry.Get(edge.pointIDA).SpatialPoint;
                var pointB = pointRegistry.Get(edge.pointIDB).SpatialPoint;
                UnityEngine.Gizmos.DrawLine(Convert.Vector(pointA), Convert.Vector(pointB));
            }
            foreach (var point in points) {
                UnityEditor.Handles.Label(Convert.Vector(point.SpatialPoint), point.ToString());
            }
        }

        private static List<int> GetLoopOrderInterPoints(HashSet<InterEdge> planeEdges) {
            var graphEdges = new HashSet<(int, int)>();
            var graphNodes = new HashSet<int>();
            foreach (var edge in planeEdges) {
                graphNodes.Add(edge.pointIDA);
                graphNodes.Add(edge.pointIDB);
                graphEdges.Add((edge.pointIDA, edge.pointIDB));
            }
            if (GraphUtilities.CheckCircularGraph(graphEdges, graphNodes)) {
                return GraphUtilities.GetLoopOrder(graphEdges, graphNodes);
            } else {
                return null;
            }
        }

        public Dictionary<FaceID, List<Vector3>> GeneratePolygons(Matrix4x4 worldTransform) {
            UnityEngine.Profiling.Profiler.BeginSample("Generate Polygons");

            var planeIDToPlaneEdges = new Dictionary<int, HashSet<InterEdge>>();
            var allPlaneIDs = planeRegistry.GetIDs();
            foreach (var planeID in allPlaneIDs) {
                planeIDToPlaneEdges.Add(planeID, new());
            }
            var interEdges = edgeRegistry.GetValues();
            foreach (var interEdge in interEdges) {
                foreach (var planeID in interEdge.commonPlaneIDs) {
                    planeIDToPlaneEdges[planeID].Add(interEdge);
                }
            }

            var faceIDToLoopedVertices = new Dictionary<FaceID, List<Vector3>>();

            foreach (var planeID in planeIDToPlaneEdges.Keys) {
                // TODO: Fix this
                if (planeID < 6) {
                    continue;
                }
                var loopedVertices = GetLoopOrderInterPoints(planeIDToPlaneEdges[planeID])
                .Select((pointID) => pointRegistry.Get(pointID).SpatialPoint)
                .Select((point) => Vector3.Transform(point, worldTransform)).ToList();

                var rawNormal = MathUtilities.GetCWNormal(loopedVertices[0], loopedVertices[1], loopedVertices[2]);
                var transformedPlaneNormal = Vector3.TransformNormal(planeRegistry.Get(planeID).Normal, worldTransform);

                if (Vector3.Dot(rawNormal, transformedPlaneNormal) < 0f) {
                    loopedVertices.Reverse();
                }

                faceIDToLoopedVertices.Add(new FaceID(planeID), loopedVertices);
            }

            return faceIDToLoopedVertices;
        }
    }
}