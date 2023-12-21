namespace PolyArchitect.Core {
    public partial class Scene {
        // private class HCmdMakeNode<T> : IHCommand {
        //     public string cliCmdName => "make_node";

        //     // parameters
        //     private readonly Scene scene;
        //     private readonly System.Func<T> createNodeObjFunc;
        //     private readonly NodeID parent;

        //     public HCmdMakeNode(Scene scene, System.Func<T> createNodeObjFunc, NodeID parent) 
        //         => (this.scene, this.createNodeObjFunc, this.parent) = (scene, createNodeObjFunc, parent);

        //     private NodeID? nodeCreated;
        //     public void Apply() {
        //         var result = scene.makeNode<T>(createNodeObjFunc(), parent);
        //         if (result == null) {
        //             throw new System.Exception();
        //         }

        //         nodeCreated = result;
        //     }

        //     public void Undo() {
        //         scene.deleteNode(nodeCreated.Value);
        //         scene.nodesMadeCount -= 1;
        //     }

        // }

        // private class HCmdDeleteNode : IHCommand {
        //     public string cliCmdName => "delete_node";

        //     // parameters
        //     private readonly Scene scene;
        //     private readonly NodeID delNodeID;

        //     public HCmdDeleteNode(Scene scene, NodeID nodeID) {
        //         this.scene = scene;
        //         this.delNodeID = nodeID;
        //     }


        //     // set with application
        //     private object nodeObjDeleted;
        //     private NodeID parentID;
        //     public void Apply() {
        //         parentID = scene.GetParent(delNodeID).Value;
        //         nodeObjDeleted = scene.deleteNode(delNodeID);
        //     }

        //     // nodeID must be free when this runs
        //     public void Undo() {
        //         scene.makeNodeAtID<object>(nodeObjDeleted, parentID, delNodeID);
        //     }
        // }

        // public class HCmdMakeEmptyBrush : IHCommand {
        //     public string cliCmdName => throw new System.NotImplementedException();

        //     private HCmdMakeNode<Brush> hCmdMakeNode;

        //     public HCmdMakeEmptyBrush(Scene scene, NodeID brushID) {
        //     }

        //     public void Apply() {
        //         throw new System.NotImplementedException();
        //     }

        //     public void Undo() {
        //         throw new System.NotImplementedException();
        //     }
        // }
    }
}