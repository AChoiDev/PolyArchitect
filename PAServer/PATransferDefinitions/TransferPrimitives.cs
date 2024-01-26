using System.Numerics;
namespace PolyArchitect.TransferDefinitions {
    public record struct float3(float x, float y, float z) {
        public static float3 From(Vector3 vector) 
            => new(vector.X, vector.Y, vector.Z);
        public void To(out Vector3 vector) 
            => vector = new Vector3(x, y, z);
    }
    public record struct float4(float w, float x, float y, float z) {
        public static float4 From(Quaternion quat) 
            => new(quat.W, quat.X, quat.Y, quat.Z);
        public void To(out Quaternion quat) 
            => quat = new Quaternion(w, x, y, z);
    }

    public record struct PosRotScale(float3 position, float4 quatRot, float3 scale) {
        public static PosRotScale From(Matrix4x4 transform) {
            Matrix4x4.Decompose(transform, 
                out Vector3 scale, out Quaternion rotation, out Vector3 position);

            return new PosRotScale(float3.From(position), float4.From(rotation), float3.From(scale));
        }

        public void To(out Matrix4x4 matrix) {
            quatRot.To(out Quaternion numQuat);
            position.To(out Vector3 numPos);
            scale.To(out Vector3 numScale);

            matrix = Matrix4x4.CreateTranslation(numPos)
                    * Matrix4x4.CreateFromQuaternion(numQuat)
                    * Matrix4x4.CreateScale(numScale);
        }
    }
}