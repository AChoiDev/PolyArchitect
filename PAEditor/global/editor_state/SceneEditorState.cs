using System.Collections.Generic;
// using System.Text.Json;
using PolyArchitect.TransferDefinitions;
namespace PolyArchitect.Editor {
    using SceneID = string;
    using NodeID = int;
    public static class SceneEditorState {
        public static Dictionary<(SceneID, NodeID), object> idToNodeState = [];

        public static void RegisterSubscriptions() {
            PAWorkerInterface.StaticSubscribe<BrushUpdateState>(
                nameof(IWorkerClient.BrushUpdate), HandleDirectUpdate<BrushUpdateState, BrushEditorState>);
            PAWorkerInterface.StaticSubscribe<NodeUpdateState>(
                nameof(IWorkerClient.BrushUpdate), HandleIndirectUpdate<NodeUpdateState, BrushEditorState>);
        }

        // Directly converts the update state into the editor state
        private static void HandleDirectUpdate<C, E>(C updateState) 
            where C : INodeAddressable
            where E : class, IDirectUpdateListener<C, E> {

            var stateKey = updateState.Address;
            var transformedState = E.Update(updateState);
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

            var stateKey = updateState.Address;

            if (idToNodeState.ContainsKey(stateKey) && idToNodeState[stateKey] is E) {
                var currentState = (E)idToNodeState[stateKey];
                idToNodeState[stateKey] = currentState.Update(updateState);
            }
        }

        public static T Get<T>(SceneID sceneID, NodeID nodeID) {
            return (T)idToNodeState[(sceneID, nodeID)];
        }


    }
}
