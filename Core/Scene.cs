using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System;


namespace PolyArchitect.Core {
    public partial class Scene : INodeObject {
        private Dictionary<NodeID, List<NodeID>> childrenOfNode = new();
        private Dictionary<NodeID, NodeID> parentOfNode = new();
        private Dictionary<NodeID, object> objectOfNodeID = new();

        private Dictionary<NodeID, Brush> brushOfNode = new();
        public Matrix4x4 LocalTransform { get => Matrix4x4.Identity; }

        private readonly NodeID sceneNodeID;
        private int nodesMadeCount = 0;
        public Scene() {
            sceneNodeID = makeNode<Scene>(this, null).Value;
        }

        // unchecked internal method
        private void makeNodeAtID<T>(T nodeObj, NodeID? parent, NodeID madeID) {
            // register node and object into tree data structure
            childrenOfNode.Add(madeID, new());
            // the condition is true iff we are creating the scene node
            if (parent.HasValue) {
                parentOfNode.Add(madeID, parent.Value);
                childrenOfNode[parent.Value].Add(madeID);
            }
            objectOfNodeID.Add(madeID, nodeObj);
        }

        private NodeID? makeNode<T>(T nodeObj, NodeID? parent) {
            if (parent.HasValue == true && IsNodeValid(parent.Value) == false) {
                return null;
            }

            var madeID = new NodeID(nodesMadeCount);
            nodesMadeCount += 1;

            makeNodeAtID<T>(nodeObj, parent, madeID);

            return madeID;
        }

        private object deleteNode(NodeID nodeID) {
            if (nodeID == sceneNodeID || IsNodeValid(nodeID) == true) {
                throw new System.Exception();
            }

            var objOfNode = objectOfNodeID[nodeID];

            childrenOfNode.Remove(nodeID);
            parentOfNode.Remove(nodeID);
            childrenOfNode[GetParent(nodeID).Value].Remove(nodeID);
            objectOfNodeID.Remove(nodeID);

            return objOfNode;
        }

        public NodeID? GetParent(NodeID id) {
            if (id == sceneNodeID || IsNodeValid(id) == false) {
                return null;
            }
            return parentOfNode[id];
        }

        public bool IsNodeValid(NodeID id) => childrenOfNode.ContainsKey(id);

        public List<NodeID> GetChildren(NodeID id) {
            if (IsNodeValid(id) == false) {
                return null;
            }
            return childrenOfNode[id].ToList();
        }

        public T Get<T>(NodeID id) where T : class, INodeObject {
            if (IsNodeValid(id) == false) {
                return null;
            }

            return objectOfNodeID[id] as T;
        }


    }
}