using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PolyArchitect.Core {

    public class MyMeshUtilities {

        public static float RandRange(System.Random randGen, float min, float max) {
            return (max - min) * (float)randGen.NextDouble() + min;

        }

        public static MyMesh CombineMeshes(List<MyMesh> myMeshes) {
            var combinedVertices = new List<Vector3>();
            var combinedIndices = new List<int>();
            var combinedColors = new List<Vector4>();
            var combinedNormals = new List<Vector3>();
            var combinedTexCoords = new List<Vector2>();
            foreach (var myMesh in myMeshes) {
                var offset = combinedVertices.Count;
                var offsetedIndices = myMesh.indices.Select((index) => index + offset);
                combinedVertices.AddRange(myMesh.vertices);
                combinedIndices.AddRange(offsetedIndices);
                if (myMesh.colors != null) {
                    combinedColors.AddRange(myMesh.colors);
                }
                combinedNormals.AddRange(myMesh.normals);
                if (myMesh.texCoords != null) {
                    combinedTexCoords.AddRange(myMesh.texCoords);
                }
            }
            var combinedMesh = new MyMesh {
                vertices = (combinedVertices),
                normals = (combinedNormals),
                colors = (combinedColors),
                indices = (combinedIndices),
                texCoords = (combinedTexCoords)
            };
            return combinedMesh;
        }
        public static MyMesh GeneratePolygonColoredMesh(List<Vector3> vertices, List<(int, int, int, int)> baseTris) {
            UnityEngine.Profiling.Profiler.BeginSample("generate colored mesh");
            var mesh = new MyMesh();

            var explodedVertices = new List<Vector3>();
            var explodedIndices = new List<int>();
            var explodedColors = new List<Vector4>();
            var explodedNormals = new List<Vector3>();
            var polygonIndexToColor = new Dictionary<int, Vector4>();
            var polygonSet = baseTris.Select((tuple) => tuple.Item4).ToHashSet();

            var rand = new System.Random(999);
            foreach (var polygon in polygonSet) {
                var color = new Vector4(RandRange(rand, 0.2f, 1f), RandRange(rand, 0.2f, 1.0f), RandRange(rand, 0.2f, 1.0f), 1.0f);

                polygonIndexToColor.Add(polygon, color);
                // polygonIndexToColor.Add(polygon, Color.blue);
            }
            // baseTris.Select((baseTri) => )
            foreach (var baseTri in baseTris) {
                var v0 = vertices[baseTri.Item1];
                var v1 = vertices[baseTri.Item2];
                var v2 = vertices[baseTri.Item3];
                var normal = MathUtilities.GetCWNormal(v0, v1, v2);

                explodedVertices.Add(v0);
                explodedIndices.Add(explodedVertices.Count - 1);

                explodedVertices.Add(v1);
                explodedIndices.Add(explodedVertices.Count - 1);

                explodedVertices.Add(v2);
                explodedIndices.Add(explodedVertices.Count - 1);


                explodedNormals.Add(normal);
                explodedNormals.Add(normal);
                explodedNormals.Add(normal);
                explodedColors.Add(polygonIndexToColor[baseTri.Item4]);
                explodedColors.Add(polygonIndexToColor[baseTri.Item4]);
                explodedColors.Add(polygonIndexToColor[baseTri.Item4]);
                // Debug.Log(polygonIndexToColor[baseTri.Item4]);
            }
            mesh.vertices = (explodedVertices);
            mesh.normals = (explodedNormals);
            mesh.colors = (explodedColors);
            mesh.indices = (explodedIndices);
            UnityEngine.Profiling.Profiler.EndSample();
            return mesh;
        }
    }
}