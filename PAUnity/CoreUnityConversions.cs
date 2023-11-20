using System.Linq;
public static class Convert {
    public static UnityEngine.Vector4 Vector(System.Numerics.Vector4 vector) {
        return new UnityEngine.Vector4(vector.X, vector.Y, vector.Z, vector.W);
    }
    public static UnityEngine.Vector3 Vector(System.Numerics.Vector3 vector) {
        return new UnityEngine.Vector3(vector.X, vector.Y, vector.Z);
    }
    public static System.Numerics.Vector3 Vector(UnityEngine.Vector3 vector) {
        return new System.Numerics.Vector3(vector.x, vector.y, vector.z);
    }

    public static System.Numerics.Matrix4x4 Transform(UnityEngine.Transform transform) {
        var upDir = Convert.Vector(transform.up);
        var frontDir = Convert.Vector(transform.forward);
        var size = Convert.Vector(transform.lossyScale);
        var pos = Convert.Vector(transform.position);

        var matTransform = 
            System.Numerics.Matrix4x4.CreateScale(size)
            * System.Numerics.Matrix4x4.CreateWorld(pos, frontDir, upDir);
        return matTransform;
    }

    public static UnityEngine.Mesh Mesh(PolyArchitect.Core.MyMesh myMesh) {
        var mesh = new UnityEngine.Mesh();
        mesh.SetVertices(myMesh.vertices.Select((v) => Convert.Vector(v)).ToList());
        mesh.SetColors(myMesh.colors.Select((v) => Convert.Vector(v)).Select((v) => new UnityEngine.Color(v.x, v.y, v.z, v.w)).ToList());
        mesh.SetNormals(myMesh.normals.Select((v) => Convert.Vector(v)).ToList());
        mesh.SetTriangles(myMesh.indices, 0);
        mesh.SetUVs(0, myMesh.texCoords.Select((v) => new UnityEngine.Vector2(v.X, v.Y)).ToList());
        return mesh;
    }
}