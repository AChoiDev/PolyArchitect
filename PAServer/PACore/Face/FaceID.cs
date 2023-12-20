namespace PolyArchitect.Core
{
    
    public struct FaceID {
        private readonly int ID;
        public FaceID(int ID) {
            this.ID = ID;
        }
        public static explicit operator FaceID(int ID) => new FaceID(ID);
        public static bool operator ==(FaceID lhs, FaceID rhs) => lhs.ID == rhs.ID;
        public static bool operator !=(FaceID lhs, FaceID rhs) => lhs.ID != rhs.ID;
    }
}
