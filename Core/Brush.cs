// stores the entire context related to a brush
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace PolyArchitect.Core {
// A convex polyhedron with UV coordinates, material IDs, and cached data
// the leaf of a csg tree
public partial class Brush {
    private readonly ConvexPolyhedron polyhedron;
    private BrushCategorizer categorizer;
    private PlanePolygons polygons;
    private PolygonSides sides;
    private Matrix4x4 worldTransform;
    private AxisAlignedBoundingBox aabb;
    public int ShapeEditCount {get; private set;}

    private readonly Dictionary<int, Vector3> planeIDToTexDir;

    public List<Vector3> GetVertexList() {
        return polygons.Vertices;
    }

    private Brush(List<Plane> planes, List<Vector3> planeTexDir, Matrix4x4 worldTransform) {
        this.worldTransform = worldTransform;
        List<int> planeIDs;
        (this.polyhedron, planeIDs) =  ConvexPolyhedron.Construct(planes);
        planeIDToTexDir = new();
        for (int i = 0; i < planes.Count; i++)
        {
            planeIDToTexDir.Add(planeIDs[i], Vector3.TransformNormal(planeTexDir[i], worldTransform));
        }
        categorizer = new BrushCategorizer(this.polyhedron, worldTransform);
        this.ShapeEditCount = 1;
        aabb = new AxisAlignedBoundingBox(polyhedron.GetTransformedVertices(worldTransform));
        polygons = new PlanePolygons(polyhedron.GeneratePolygons(worldTransform));
        sides = new PolygonSides(polygons.GetPolygonIDs(), polygons);
    }


    public void ChangeTransform(Matrix4x4 worldTransform) {
        this.worldTransform = worldTransform;
        categorizer = new BrushCategorizer(this.polyhedron, worldTransform);
        ShapeEditCount += 1;
        aabb = new AxisAlignedBoundingBox(polyhedron.GetTransformedVertices(worldTransform));
        polygons = new PlanePolygons(polyhedron.GeneratePolygons(worldTransform));
        sides = new PolygonSides(polygons.GetPolygonIDs(), polygons);
    }

    public PolygonSide Categorize(ConvexPolygon polygon) {
        return categorizer.CategorizePolygon(polygon);
    }

    public static bool DoesAABBCollide(Brush brushOne, Brush brushTwo) {
        return AxisAlignedBoundingBox.DoesCollide(brushOne.aabb, brushTwo.aabb);
    }

    public void Slice(HashSet<Brush> otherBrushes) {
        polygons = new PlanePolygons(polyhedron.GeneratePolygons(worldTransform));
        foreach (var otherBrush in otherBrushes) {
            polygons.SliceSelfWith(otherBrush.polygons);
        }
        sides = new PolygonSides(polygons.GetPolygonIDs(), polygons);
    }


    public List<(int, int, int, int)> GenerateTris() {
        var validPolygons = sides.GetValidPolygons();
        foreach (var validPolygon in validPolygons) {
            var center = validPolygon.AveragePos;
            var planeID = validPolygon.originatingPlaneID;
            var uDir = planeIDToTexDir[planeID];
            var normalDir = Vector3.Normalize(Vector3.TransformNormal(polyhedron.GetPlaneNormal(planeID), worldTransform));
            var vDir = -Vector3.Cross(uDir, normalDir);
            UnityEngine.Gizmos.DrawLine(Convert.Vector(center), Convert.Vector(center + uDir * 0.1f));
            UnityEngine.Gizmos.DrawLine(Convert.Vector(center), Convert.Vector(center + normalDir * 0.1f));
            UnityEngine.Gizmos.DrawLine(Convert.Vector(center), Convert.Vector(center + vDir * 0.1f));
        }
        return sides.GenerateTris();
    }

    public MyMesh GenerateMesh() {
        var meshVertices = new List<Vector3>();
        var meshIndices = new List<int>();
        var meshNormals = new List<Vector3>();
        var meshTexCoords = new List<Vector2>();
        var planeIDToPolygonList = new Dictionary<int, List<ConvexPolygon>>();
        var validPolygons = sides.GetValidPolygons();
        foreach (var polygon in validPolygons) {
            var planeID = polygon.originatingPlaneID;
            if (planeIDToPolygonList.ContainsKey(planeID) == false) {
                planeIDToPolygonList.Add(planeID, new());
            }
            planeIDToPolygonList[planeID].Add(polygon);
        }



        foreach (var polygonList in planeIDToPolygonList.Values) {
            GenerateTrisOfPlane(polygonList, meshVertices, meshIndices, meshNormals, meshTexCoords);
        }

        var mesh = new MyMesh(){
            indices = meshIndices,
            vertices = meshVertices,
            normals = meshNormals,
            texCoords = meshTexCoords,
        };

        return mesh;
    }

    private void GenerateTrisOfPlane(List<ConvexPolygon> polygons, 
        List<Vector3> meshVertices, List<int> meshIndices, 
        List<Vector3> meshNormals, List<Vector2> meshTexCoords) {
        var planeID = polygons[0].originatingPlaneID;

        var vertexIDToIndex = new Dictionary<int, int>();

        foreach (var polygon in polygons) {
            var vertices = polygon.FetchVertices();
            var uDir = planeIDToTexDir[planeID];
            var vDir = -Vector3.Cross(uDir, polygon.Normal);
            for (int i = 0; i < vertices.Count; i++)
            {
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

    public void CategorizePolygons(CSGTree tree) {
        sides.CategorizeFromTree(tree);
    }

}
}