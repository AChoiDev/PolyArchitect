public readonly struct NodeID {
    private readonly int ID;
    public NodeID(int ID) {
        this.ID = ID;
    }
    public static explicit operator NodeID(int ID) => new NodeID(ID);
    public static bool operator ==(NodeID lhs, NodeID rhs) => lhs.ID == rhs.ID;
    public static bool operator !=(NodeID lhs, NodeID rhs) => lhs.ID != rhs.ID;
}