using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PolyArchitect.Core {

    // manages the polygons of the brush
    // TODO: investigate just having a class represent all the polygons of a single plane
    // instead of an object that manages all polygons of all planes of a brush
    public class PlanePolygons {
        private readonly Dictionary<int, HashSet<int>> planeIDToPolygonIDs;
        public readonly HashSet<(int, ConvexPolygon)> BasePolygons;

        private readonly Registry<ConvexPolygon> polygons;

        public readonly List<Vector3> Vertices;

        public ConvexPolygon GetPolygon(int ID) {
            return polygons.Get(ID);
        }

        public Vector3 GetVertex(int ID) {
            return Vertices[ID];
        }

        public int RegisterVertex(Vector3 vertex) {
            Vertices.Add(vertex);
            return Vertices.Count - 1;
        }

        public ConvexPolygon MakePolygon(List<Vector3> loopedVertices, int originalPlaneID) {
            var loopedVertexIndices = loopedVertices.Select((vertex) => RegisterVertex(vertex)).ToList();
            var polygon = new ConvexPolygon(loopedVertexIndices, this, originalPlaneID);
            return polygon;
        }

        public PlanePolygons(Dictionary<int, List<Vector3>> planeIDToLoopedVertices) {
            polygons = new();
            planeIDToPolygonIDs = new();
            Vertices = new();
            BasePolygons = new();

            foreach (var planeID in planeIDToLoopedVertices.Keys) {
                var basePolygon = MakePolygon(planeIDToLoopedVertices[planeID], planeID);
                BasePolygons.Add((planeID, basePolygon));
                var basePolygonID = polygons.Add(basePolygon);
                planeIDToPolygonIDs.Add(planeID, new() { basePolygonID });
            }
        }

        public void SlicePolygonsInPlane(int targetPlane, Plane slicingPlane) {
            var polygonIDSet = planeIDToPolygonIDs[targetPlane];
            var slicedPolygons = new HashSet<int>();
            foreach (var polygonID in polygonIDSet) {
                polygons.Get(polygonID).Slice(slicingPlane).ForEach((polygon) => slicedPolygons.Add(polygons.Add(polygon)));
                polygons.Remove(polygonID);
            }
            planeIDToPolygonIDs[targetPlane] = slicedPolygons;
        }

        public Dictionary<int, HashSet<Plane>> MakeSliceDictionary(PlanePolygons otherBrushPolygons) {
            var slicingPolygons = otherBrushPolygons.GetBasePolygonSet();
            var sliceDictionary = new Dictionary<int, HashSet<Plane>>();
            foreach (var planeID in GetPlaneIndices()) {
                sliceDictionary.Add(planeID, new());
            }
            foreach (var slicingPolygon in slicingPolygons) {
                foreach (var pair in BasePolygons) {
                    var basePolygon = pair.Item2;
                    if (basePolygon.DoesCollide(slicingPolygon)) {
                        sliceDictionary[pair.Item1].Add(slicingPolygon.GeneratePlane());
                    }
                }
            }

            return sliceDictionary;
        }

        public void SliceSelfWith(Dictionary<int, HashSet<Plane>> sliceDictionary) {
            foreach (var targetPlaneIndex in sliceDictionary.Keys) {
                foreach (var slicePlane in sliceDictionary[targetPlaneIndex]) {
                    SlicePolygonsInPlane(targetPlaneIndex, slicePlane);
                }
            }
        }

        public void SliceSelfWith(PlanePolygons other) {
            var sliceDictionary = MakeSliceDictionary(other);
            SliceSelfWith(sliceDictionary);
        }

        public HashSet<ConvexPolygon> GetPolygonSet() {
            return polygons.GetValues().ToHashSet();
        }

        public HashSet<int> GetPolygonIDs() => polygons.GetIDs();

        public HashSet<ConvexPolygon> GetPolygonsOfPlaneID(int planeID) {
            return planeIDToPolygonIDs[planeID].Select((id) => polygons.Get(id)).ToHashSet();
        }

        public HashSet<ConvexPolygon> GetBasePolygonSet() {
            return BasePolygons.Select((pair) => pair.Item2).ToHashSet();
        }

        public HashSet<int> GetPlaneIndices() {
            return planeIDToPolygonIDs.Keys.ToHashSet();
        }

    }
}