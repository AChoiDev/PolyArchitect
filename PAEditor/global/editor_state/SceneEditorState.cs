using System.Collections.Generic;
// using System.Text.Json;
using PolyArchitect.TransferDefinitions;
namespace PolyArchitect.Editor {
    using SceneID = string;
    using NodeID = int;
    public static class SceneEditorState {
        public static Dictionary<(SceneID, NodeID), object> idToNodeState = [];

        public static void RegisterSubscriptions() {
            // brush editor state subscriptions
            PAWorkerInterface.StaticSubscribe<BrushUpdateState>(
                nameof(IWorkerClient.BrushUpdate), HandleDirectUpdate<BrushUpdateState, BrushEditorState>);
            PAWorkerInterface.StaticSubscribe<CSGLinkUpdateState>(
                nameof(IWorkerClient.NodeUpdate), HandleIndirectUpdate<CSGLinkUpdateState, BrushEditorState>);
            PAWorkerInterface.StaticSubscribe<NodeUpdateState>(
                nameof(IWorkerClient.NodeUpdate), HandleIndirectUpdate<NodeUpdateState, BrushEditorState>);

            // csglink state subscriptions
            PAWorkerInterface.StaticSubscribe<CSGLinkUpdateState>(
                nameof(IWorkerClient.NodeUpdate), HandleDirectUpdate<CSGLinkUpdateState, CSGLinkEditorState>);
            PAWorkerInterface.StaticSubscribe<NodeUpdateState>(
                nameof(IWorkerClient.NodeUpdate), HandleIndirectUpdate<NodeUpdateState, CSGLinkEditorState>);
        }

        // Directly converts the update state into the editor state
        private static void HandleDirectUpdate<C, E>(C updateState) 
            where C : INodeAddressable
            where E : class, IDirectUpdateListener<C, E> {

            var stateKey = (updateState.SceneID, updateState.NodeID);
            var transformedState = E.FromUpdate(updateState);
            if (idToNodeState.ContainsKey(stateKey)) {
                idToNodeState[stateKey] = transformedState;
            } else {
                idToNodeState.Add(stateKey, transformedState);
            }
        }

        // Uses the current state specified by the update state to 
        private static void HandleIndirectUpdate<C, E>(C updateState) 
            where C : INodeAddressable
            where E : class, IIndirectUpdateListener<C, E> {

            var stateKey = (updateState.SceneID, updateState.NodeID);

            if (idToNodeState.ContainsKey(stateKey) && idToNodeState[stateKey] is E) {
                var currentState = (E)idToNodeState[stateKey];
                idToNodeState[stateKey] = currentState.WithUpdate(updateState);
            }
        }

        public static T Get<T>(SceneID sceneID, NodeID nodeID) {
            return (T)idToNodeState[(sceneID, nodeID)];
        }


    }
}
