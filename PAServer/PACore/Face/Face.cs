using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PolyArchitect.Core {
    public class Face : IVertexManager {
        public class FacePolygon {
            public ConvexPolygonGeometry geometry;
            public PolygonSide side;
        }
        public readonly ConvexPolygonGeometry baseGeometry;

        public readonly Registry<Vector3> Vertices = new();
        private readonly Registry<FacePolygon> polygons = new();
        private Vector3 texDir;

        private int AddPolygon(ConvexPolygonGeometry paramGeometry) {
            var insertPolygon = new FacePolygon(){
                geometry=paramGeometry, 
                side=PolygonSide.ALIGNED
            };
            return polygons.Add(insertPolygon);
        }

        private void RemovePolygon(int ID) {
            polygons.Remove(ID);
        }

        private ConvexPolygonGeometry MakePolygonGeometry(List<Vector3> loopedVertices) {
            var loopedVertexIndices = loopedVertices.Select((vertex) => Vertices.Add(vertex)).ToList();
            var polygon = new ConvexPolygonGeometry(loopedVertexIndices, this);
            return polygon;
        }

        public Face(List<Vector3> loopedVertices) {
            texDir = Vector3.Normalize(Vector3.One + Vector3.UnitX);
            baseGeometry = MakePolygonGeometry(loopedVertices);
            AddPolygon(baseGeometry);
        }

        public void Slice(Plane slicingPlane) {
            foreach (var ID in polygons.GetIDs()) {
                var slicedPolygons = polygons.Get(ID).geometry.Slice(slicingPlane);
                RemovePolygon(ID);
                slicedPolygons.ForEach((slice) => AddPolygon(slice));
            }
        }

        public void SetSidesFromTree(CSGTree tree) {
            foreach (var id in polygons.GetIDs()) {
                var polygon = polygons.Get(id);
                polygon.side = tree.Categorize(polygon.geometry);
            }
        }

        private List<ConvexPolygonGeometry> GetSidedGeometries() {
            var geometryList = new List<ConvexPolygonGeometry>();

            foreach (var id in polygons.GetIDs()) {
                var side = polygons.Get(id).side;
                if (side == PolygonSide.ALIGNED
                    || side == PolygonSide.REVERSE_ALIGNED) {
                    var polygonGeo = polygons.Get(id).geometry;

                    if (side == PolygonSide.REVERSE_ALIGNED) {
                        // flip triangles when reverse aligned
                        polygonGeo = polygonGeo.MakeFlipped();
                    }
                    // only add polygons if aligned or reverse aligned
                    geometryList.Add(polygonGeo);

                }
            }
            return geometryList;
        }

        public void AddTris(List<Vector3> meshVertices, List<int> meshIndices,
            List<Vector3> meshNormals, List<Vector2> meshTexCoords) {
            var sidedGeometries = GetSidedGeometries();

            var vertexIDToIndex = new Dictionary<int, int>();
            var uDir = texDir;

            foreach (var polygon in sidedGeometries) {
                var vertices = polygon.FetchVertices();
                var vDir = -Vector3.Cross(uDir, polygon.Normal);
                for (int i = 0; i < vertices.Count; i++) {
                    var vertexID = polygon.LoopedVertexIDs[i];
                    if (vertexIDToIndex.ContainsKey(vertexID) == false) {
                        var vertex = vertices[i];
                        meshVertices.Add(vertex);
                        var u = Vector3.Dot(vertex, uDir);
                        var v = Vector3.Dot(vertex, vDir);
                        meshTexCoords.Add(new(u, v));
                        meshNormals.Add(polygon.Normal);
                        var index = meshVertices.Count - 1;
                        vertexIDToIndex.Add(vertexID, index);
                    }
                }

                var triangles = polygon.MakeTriangles();
                foreach (var triplet in triangles) {
                    meshIndices.Add(vertexIDToIndex[triplet.Item1]);
                    meshIndices.Add(vertexIDToIndex[triplet.Item2]);
                    meshIndices.Add(vertexIDToIndex[triplet.Item3]);
                }
            }

        }

        public void PenDebugDrawSplitPolygons() {
            foreach (var polygon in polygons.GetValues()) {
                polygon.geometry.PenDebugDraw();
            }
        }
            

        // IVertexManager implementation
        int IVertexManager.Add(Vector3 point) => Vertices.Add(point);
        Vector3 IVertexManager.Get(int ID) => Vertices.Get(ID);
    }
}