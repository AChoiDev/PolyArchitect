using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System;

using NodeID = int;
namespace PolyArchitect.Core {
    public partial class Scene : INodeContent {
        public struct Node(INodeContent content, NodeID parent, List<NodeID> children, Matrix4x4 transform)
        {
            public INodeContent Content {get;} = content;
            public NodeID Parent {get; set;} = parent;
            public List<NodeID> Children {get;} = children;
            public Matrix4x4 Transform {get; set;} = transform;
        }
        private readonly Dictionary<NodeID, Node> nodeRegistry = [];
        private int nodeIDIncrementor = 0;

        private const NodeID ROOT_NODE_ID = 100_000_000;
        private const NodeID NONE_NODE_ID = ROOT_NODE_ID + 1;

        public Scene() {
            makeNodeAtID(this, NONE_NODE_ID, ROOT_NODE_ID);
        }

        // unchecked internal method
        private void makeNodeAtID(INodeContent nodeObj, NodeID parent, NodeID madeID) {
            // register node and object into tree data structure
            nodeRegistry.Add(madeID, new Node(nodeObj, parent, [], Matrix4x4.Identity));
            if (madeID != ROOT_NODE_ID) {
                nodeRegistry[parent].Children.Add(madeID);
            }
        }

        public NodeID MakeNode(INodeContent nodeObj, NodeID? parent = null) {
            var madeID = (NodeID)nodeIDIncrementor;
            nodeIDIncrementor += 1;

            makeNodeAtID(nodeObj, parent ?? ROOT_NODE_ID, madeID);

            return madeID;
        }


        public NodeID? GetParent(NodeID id) {
            if (IsNodeValid(id) == false) {
                throw new Exception("Node not valid");
            }
            var rawParentID = nodeRegistry[id].Parent;
            if (rawParentID == ROOT_NODE_ID) {
                return null;
            } else {
                return rawParentID;
            }
            
        }

        public bool IsNodeValid(NodeID id) => nodeRegistry.ContainsKey(id);

        public List<NodeID> GetChildren(NodeID id) {
            if (IsNodeValid(id) == false) {
                throw new Exception("Node not valid");
            }
            return nodeRegistry[id].Children;
        }

        public int GetChildCount(NodeID id) {
            if (IsNodeValid(id) == false) {
                throw new System.Exception("NodeID does not exist");
            }
            return GetChildren(id).Count;
        }

        public T? GetOrNull<T>(NodeID id) where T : class, INodeContent {
            if (IsNodeValid(id) == false) {
                return null;
            }

            if (nodeRegistry[id].Content is T nc) {
                return nc;
            } else {
                return null;
            }
        }

        public T Get<T>(NodeID id) where T : INodeContent {
            if (IsNodeValid(id) == false ) {
                throw new System.Exception("NodeID does not exist");
            }

            if (nodeRegistry[id].Content is T nc) {
                return nc;
            } else {
                throw new System.Exception("NodeID is not type " + typeof(T).Name);
            }
        }

        public Matrix4x4 GetLocalTransform(NodeID nodeID) => nodeRegistry[nodeID].Transform;
        public void SetLocalTransform(NodeID nodeID, Matrix4x4 transform) {
            var node = nodeRegistry[nodeID];
            node.Transform = transform;
        } 

        public Matrix4x4 GetGlobalTransform(NodeID nodeID) {
            List<Matrix4x4> matrices = [];
            NodeID? current = nodeID;
            while (current.HasValue) {
                matrices.Add(GetLocalTransform(current.Value));
                current = GetParent(current.Value);
            }
            matrices.Reverse();
            return matrices.Aggregate((a, b) => a * b);
        }

        public CSGTree Traverse(NodeID nodeID, CSGTree parentCSGTree) {
            var currentCSGTree = parentCSGTree;
            var brushNode = GetOrNull<Brush>(nodeID);
            if (brushNode != null) {
                currentCSGTree = new CSGTree(brushNode.operation, "", brushNode);
                parentCSGTree.AddTree(currentCSGTree);
            }
            var opNodeComp = GetOrNull<CSGLink>(nodeID);
            if (opNodeComp != null) {
                currentCSGTree = new CSGTree(opNodeComp.operation, "");
                parentCSGTree.AddTree(currentCSGTree);
            }
            var childrenIDs = GetChildren(nodeID);
            foreach (var childID in childrenIDs) {
                Traverse(childID, currentCSGTree);
            }

            return parentCSGTree;
        }

        private Dictionary<NodeID, HashSet<NodeID>> FindBrushCollisionLists() {
            var adjacencyList = new Dictionary<NodeID, HashSet<NodeID>>();
            var brushIDList = nodeRegistry.Keys.Where((key) => nodeRegistry[key].Content is Brush).ToList();
            
            foreach (var id in brushIDList) {
                adjacencyList.Add(id, []);
            }

            for (int i = 0; i < brushIDList.Count - 1; i++) {
                for (int j = i + 1; j < brushIDList.Count; j++) {
                    var brushIDA = brushIDList[i];
                    var brushIDB = brushIDList[j];
                    var brushA = Get<Brush>(brushIDList[i]);
                    var brushB = Get<Brush>(brushIDList[j]);

                    if (AxisAlignedBoundingBox.DoesCollide(
                            brushA.AABB(brushA.worldTransform), 
                            brushB.AABB(brushB.worldTransform))) {
                        adjacencyList[brushIDA].Add(brushIDB);
                        adjacencyList[brushIDB].Add(brushIDA);
                    }
                }
            }

            return adjacencyList;
        }

        private void SliceBrushes() {
            var brushCollisionLists = FindBrushCollisionLists();
            foreach (var (brushID, brushCollisionList) in brushCollisionLists) {
                var collidedBrushes = brushCollisionList.Select(Get<Brush>).ToHashSet();
                Get<Brush>(brushID).Slice(collidedBrushes);
            }    
        }

        private void CategorizeBrushSplitPolygons() {
            var csgTree = Traverse(ROOT_NODE_ID, new CSGTree(BooleanOperation.Add, "root"));
            var brushes = nodeRegistry.Values.Select((node) => node.Content as Brush)
                .Where((content) => content != null).ToList();
            foreach (var brush in brushes) {
                brush.CategorizePolygons(csgTree);
            }
        }

        public MyMesh GenerateMesh() {
            SliceBrushes();
            CategorizeBrushSplitPolygons();
            var brushes = nodeRegistry.Values.Select((node) => node.Content as Brush)
                .Where((content) => content != null).ToList();
            var brushMeshes = brushes.Select((brush) => brush.GenerateMesh()).ToList();
            return MyMeshUtilities.CombineMeshes(brushMeshes);
        }
    }
}