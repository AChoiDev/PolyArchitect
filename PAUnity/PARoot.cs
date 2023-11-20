using PolyArchitect.Core;
using PolyArchitect.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// main unity driver for CSG Tree
[ExecuteInEditMode()]
public class PARoot : MonoBehaviour {

    public class BrushRegistration {
        public Brush brush;
        public int ShapeEditCount;
        public bool NeedsSlice;
    }

    private Registry<BrushRegistration> brushRegistry = new();


    public static CSGTree Traverse(Transform node, CSGTree parentCSGTree) {
        var currentCSGTree = parentCSGTree;
        var brushComp = node.GetComponent<PABrush>();
        if (brushComp != null) {
            currentCSGTree = new CSGTree(brushComp.operation, node.gameObject.name, brushComp.GetBrush());
            parentCSGTree.AddTree(currentCSGTree);
        }
        var opNodeComp = node.GetComponent<PAOperationNode>();
        if (opNodeComp != null) {
            currentCSGTree = new CSGTree(opNodeComp.operation, node.gameObject.name);
            parentCSGTree.AddTree(currentCSGTree);
        }
        var childCount = node.childCount;
        foreach (var childIndex in Enumerable.Range(0, childCount)) {
            var child = node.GetChild(childIndex);
            Traverse(child, currentCSGTree);
        }

        return parentCSGTree;
    }

    public string GetCSGTreeString() {
        var csgTree = Traverse(transform, new CSGTree(BooleanOperation.Add, "root"));
        return csgTree.Print(0);
    }

    private void updateBrushPresence() {
        var csgTree = Traverse(transform, new CSGTree(BooleanOperation.Add, "root"));
        var csgTreeBrushes = csgTree.GetBrushes().ToHashSet();

        // brushes in csg tree but not in currentBrushes
        var brushesToAdd = csgTreeBrushes.Except(brushRegistry.GetValues().Select((value) => value.brush)).ToList();
        // brush IDs in brushes that no longer apply
        var brushIDsToRemove = brushRegistry.GetIDs().Where((brushID) => csgTreeBrushes.Contains(brushRegistry.Get(brushID).brush) == false).ToList();

        foreach (var brush in brushesToAdd) {
            brushRegistry.Add(new BrushRegistration() { brush = brush, ShapeEditCount = -1, NeedsSlice = false });
        }

        foreach (var brushID in brushIDsToRemove) {
            brushRegistry.Remove(brushID);
        }
    }

    private void updateBrushRegistryMetaInfo() {
        var brushIDs = brushRegistry.GetIDs();
        foreach (var brushID in brushIDs) {
            var reg = brushRegistry.Get(brushID);
            if (reg.ShapeEditCount != reg.brush.ShapeEditCount) {
                reg.ShapeEditCount = reg.brush.ShapeEditCount;

                brushRegistry.Get(brushID).NeedsSlice = true;

                var collidedBrushIDs = surfaceCollisions[brushID];
                foreach (var collidedBrushID in collidedBrushIDs) {
                    brushRegistry.Get(collidedBrushID).NeedsSlice = true;
                }
            }
        }
    }

    private Dictionary<int, HashSet<int>> FindSurfaceCollisions() {
        var adjacencyList = new Dictionary<int, HashSet<int>>();
        var brushIDList = brushRegistry.GetIDs().ToList();
        foreach (var id in brushIDList) {
            adjacencyList.Add(id, new());
        }
        for (int i = 0; i < brushIDList.Count - 1; i++) {
            for (int j = i + 1; j < brushIDList.Count; j++) {
                var brushIDA = brushIDList[i];
                var brushIDB = brushIDList[j];
                var brushA = brushRegistry.Get(brushIDList[i]).brush;
                var brushB = brushRegistry.Get(brushIDList[j]).brush;

                if (Brush.DoesAABBCollide(brushA, brushB)) {
                    adjacencyList[brushIDA].Add(brushIDB);
                    adjacencyList[brushIDB].Add(brushIDA);
                }
            }
        }

        return adjacencyList;
    }

    private void applyDirtySlices() {
        foreach (var brushID in surfaceCollisions.Keys) {
            if (brushRegistry.Get(brushID).NeedsSlice) {
                var collidedBrushes = surfaceCollisions[brushID]
                    .Select((id) => brushRegistry.Get(id).brush).ToHashSet();
                brushRegistry.Get(brushID).brush.Slice(collidedBrushes);

                brushRegistry.Get(brushID).NeedsSlice = false;
            }
        }
    }


    // public HashSet<Brush> brushes;
    [SerializeField] private MeshFilter customMesh;
    private Dictionary<int, HashSet<int>> surfaceCollisions = new();

    public void OnDrawGizmos() {
        updateBrushPresence();
        surfaceCollisions = FindSurfaceCollisions();
        updateBrushRegistryMetaInfo();
        applyDirtySlices();


        var csgTree = Traverse(transform, new CSGTree(BooleanOperation.Add, "root"));
        foreach (var brush in brushRegistry.GetValues().Select((reg) => reg.brush)) {
            // brush.DrawNormal();
            brush.CategorizePolygons(csgTree);
        }

        var myMeshes = brushRegistry.GetValues().Select((reg) => reg.brush).Select((brush) => brush.GenerateMesh()).ToList();
        customMesh.mesh = Convert.Mesh(MyMeshUtilities.CombineMeshes(myMeshes));

    }

}