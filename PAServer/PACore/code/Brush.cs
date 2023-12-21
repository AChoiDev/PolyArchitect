// stores the entire context related to a brush
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using FaceID = int;

namespace PolyArchitect.Core {
    // A convex polyhedron with UV coordinates, material IDs, and cached data
    // the leaf of a csg tree
    public partial class Brush : INodeContent {
        private readonly ConvexPolyhedron polyhedron;
        private BrushCategorizer categorizer;
        private Dictionary<FaceID, Face> faces;
        public Matrix4x4 worldTransform {get; set;}
        public int ShapeEditCount { get; private set; }

        public AxisAlignedBoundingBox AABB(Matrix4x4 worldTransform)
            => new (polyhedron.GetTransformedVertices(worldTransform));

        public readonly BooleanOperation operation;

        public Brush(List<Plane> planes, Matrix4x4 worldTransform, BooleanOperation operation) {
            this.operation = operation;
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
            GenerateFaces();
        }

        public PolygonSide Categorize(ConvexPolygonGeometry polygon) {
            return categorizer.CategorizePolygon(polygon);
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
            Dictionary<FaceID, ConvexPolygonGeometry> basePolygons = [];
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

            // foreach (var face in faces) {
            //     face.Value.PenDebugDrawSplitPolygons();
            // }
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