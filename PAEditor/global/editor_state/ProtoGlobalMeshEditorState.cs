using PolyArchitect.TransferDefinitions;
using System.Linq;
using Godot;
namespace PolyArchitect.Editor {
    public record ProtoGlobalMeshEditorState(Vector3[] Vertices, int[] Indices, Vector3[] Normals) {
		public static Vector3 Convert(float3 val) 
			=> new(val.x, val.y, val.z);
        public static ProtoGlobalMeshEditorState FromUpdate(ProtoGlobalMeshUpdateState update) {
            return new(
                Vertices: update.Vertices.Select(Convert).ToArray(),
                Indices: FlipIndexOrder(update.Indices),
                Normals: update.Normals.Select(Convert).ToArray()
            );
        }

        public static int[] FlipIndexOrder(int[] indices) {
            var flipIndices = new int[indices.Length];
            for (int i = 0; i < indices.Length; i += 3)
            {
                (flipIndices[i], flipIndices[i+1], flipIndices[i+2]) = (indices[i+2], indices[i+1], indices[i]);
            }
            return flipIndices;
        }

        public ArrayMesh MakeGodotMesh() {
            // Initialize the ArrayMesh.
            var arrMesh = new ArrayMesh();
            var arrays = new Godot.Collections.Array();
            arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = Vertices;
            arrays[(int)Mesh.ArrayType.Index] = Indices;
            arrays[(int)Mesh.ArrayType.Normal] = Normals;

            foreach (var vert in Vertices) {
                GD.Print(vert);
            }

            // Create the Mesh.
            arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
            return arrMesh;
        }
    }
}