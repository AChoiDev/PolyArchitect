// stores the entire context related to a brush
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PolyArchitect.Core {
    // A convex polyhedron with UV coordinates, material IDs, and cached data
    // the leaf of a csg tree
    public partial class Brush {
        private readonly ConvexPolyhedron polyhedron;
        private BrushCategorizer categorizer;
        private Dictionary<FaceID, Face> faces;
        private Matrix4x4 worldTransform;
        private AxisAlignedBoundingBox aabb;
        public int ShapeEditCount { get; private set; }

        private Brush(List<Plane> planes, List<Vector3> planeTexDir, Matrix4x4 worldTransform) {
            // List<int> planeIDs;
            (polyhedron, _) = ConvexPolyhedron.Construct(planes);
            // TODO: Fix this
            // Dictionary<FaceID, Vector3> faceIDToTexDir = new();
            // for (int i = 0; i < planes.Count; i++) {
                // faceIDToTexDir.Add(planeIDs[i], Vector3.TransformNormal(planeTexDir[i], worldTransform));
            // }

            ChangeTransform(worldTransform);
        }

        private void GenerateFaces() {
            var generatedPolygons = polyhedron.GeneratePolygons(worldTransform);
            faces = new();
            foreach (var (faceID, loopedVertices) in generatedPolygons) {
                faces.Add(faceID, new Face(loopedVertices));
            }
        }


        public void ChangeTransform(Matrix4x4 worldTransform) {
            this.worldTransform = worldTransform;
            categorizer = new BrushCategorizer(this.polyhedron, worldTransform);
            ShapeEditCount += 1;
            aabb = new AxisAlignedBoundingBox(polyhedron.GetTransformedVertices(worldTransform));
            GenerateFaces();
        }

        public PolygonSide Categorize(ConvexPolygonGeometry polygon) {
            return categorizer.CategorizePolygon(polygon);
        }

        public static bool DoesAABBCollide(Brush brushOne, Brush brushTwo) {
            return AxisAlignedBoundingBox.DoesCollide(brushOne.aabb, brushTwo.aabb);
        }

        private Dictionary<FaceID, HashSet<Plane>> MakeSliceDictionary(HashSet<ConvexPolygonGeometry> slicingPolygons) {
            var sliceDictionary = new Dictionary<FaceID, HashSet<Plane>>();
            foreach (var faceID in faces.Keys) {
                sliceDictionary.Add(faceID, new());
            }
            var basePolygons = GetBasePolygons();
            foreach (var slicingPolygon in slicingPolygons) {
                foreach (var (faceID, faceBasePolygon) in basePolygons) {
                    if (faceBasePolygon.DoesCollide(slicingPolygon)) {
                        sliceDictionary[faceID].Add(slicingPolygon.GeneratePlane());
                    }
                }
            }

            return sliceDictionary;
        }

        private Dictionary<FaceID, ConvexPolygonGeometry> GetBasePolygons() {
            Dictionary<FaceID, ConvexPolygonGeometry> basePolygons = new();
            foreach (var (faceID, face) in faces) {
                basePolygons.Add(faceID, face.baseGeometry);
            }
            return basePolygons;
        }

        public void Slice(HashSet<Brush> otherBrushes) {
            GenerateFaces();

            foreach (var otherBrush in otherBrushes) {
                var slicingPolygons = otherBrush.GetBasePolygons().Values.ToHashSet();
                var sliceDictionary = MakeSliceDictionary(slicingPolygons);
                foreach (var targetFaceID in sliceDictionary.Keys) {
                    foreach (var slicePlane in sliceDictionary[targetFaceID]) {
                        faces[targetFaceID].Slice(slicePlane);
                    }
                }
            }
        }


        public MyMesh GenerateMesh() {
            var meshVertices = new List<Vector3>();
            var meshIndices = new List<int>();
            var meshNormals = new List<Vector3>();
            var meshTexCoords = new List<Vector2>();

            foreach (var face in faces.Values) {
                face.AddTris(meshVertices, meshIndices, meshNormals, meshTexCoords);
            }

            var mesh = new MyMesh() {
                indices = meshIndices,
                vertices = meshVertices,
                normals = meshNormals,
                texCoords = meshTexCoords,
            };

            return mesh;
        }



        public void CategorizePolygons(CSGTree tree) {
            foreach (var face in faces.Values) {
                face.SetSidesFromTree(tree);
            }
        }

    }
}