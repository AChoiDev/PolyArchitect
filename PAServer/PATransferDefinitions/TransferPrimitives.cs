
namespace PolyArchitect.TransferDefinitions {
    public readonly struct float3(float x, float y, float z) {
        public readonly float x = x;
        public readonly float y = y;
        public readonly float z = z;
    }
    public readonly struct float4(float w, float x, float y, float z) {
        public readonly float w = w;
        public readonly float x = x;
        public readonly float y = y;
        public readonly float z = z;
    }
}