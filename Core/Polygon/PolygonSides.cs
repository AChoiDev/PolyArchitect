using System.Collections.Generic;
using System.Linq;

namespace PolyArchitect.Core {
    // manages the sides of the polygons
    // TODO: merge this with the planepolygons class
    public class PolygonSides {
        private Dictionary<int, PolygonSide> polygonIDToSide;
        private PlanePolygons polygons;

        public PolygonSides(HashSet<int> polygonIDs, PlanePolygons polygons) {
            polygonIDToSide = new();
            this.polygons = polygons;
            foreach (var id in polygonIDs) {
                polygonIDToSide.Add(id, PolygonSide.ALIGNED);
            }
        }

        // infer the sideness of the polygons from the csg tree
        public void CategorizeFromTree(CSGTree tree) {
            var modDic = new Dictionary<int, PolygonSide>();
            foreach (var id in polygonIDToSide.Keys) {
                var polygon = polygons.GetPolygon(id);
                modDic.Add(id, tree.Categorize(polygon));
            }
            polygonIDToSide = modDic;
        }

        // Get all polygons that are on the surface of the CSG shape
        // make a flipped variant if they are reverse aligned
        public List<ConvexPolygon> GetValidPolygons() {
            var polygonList = new List<ConvexPolygon>();
            foreach (var (id, side) in polygonIDToSide) {
                if (side == PolygonSide.ALIGNED
                    || side == PolygonSide.REVERSE_ALIGNED) {
                    var polygon = polygons.GetPolygon(id);

                    if (side == PolygonSide.REVERSE_ALIGNED) {
                        // flip triangles when reverse aligned
                        polygon = polygon.MakeFlipped();
                    }
                    // only add polygons if aligned or reverse aligned
                    polygonList.Add(polygon);

                }
            }
            return polygonList;
        }

        public List<(int, int, int, int)> GenerateTris() {
            var triList = new List<(int, int, int, int)>();

            var validPolygons = GetValidPolygons();

            int i = 0;
            foreach (var polygon in validPolygons) {
                var tris = polygon.MakeTriangles()
                    .Select((tri) => (tri.Item1, tri.Item2, tri.Item3, i)).ToList();
                triList.AddRange(tris);
                i++;
            }

            return triList;
        }
    }
}