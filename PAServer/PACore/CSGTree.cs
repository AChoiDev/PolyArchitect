using System;
using System.Collections.Generic;
using System.Linq;

namespace PolyArchitect.Core {

    public enum BooleanOperation { Add, Subtract, Intersect }

    // A tree of CSG shapes
    // TODO: check all leaves of trees are brushes
    // and check all non-leaves are non brushes
    // and infer subset tree from intersecting brushes
    public class CSGTree {
        private readonly List<CSGTree> subTrees;
        public string Name { get; private set; }

        public BooleanOperation operation;
        private readonly Brush brush;


        public CSGTree(BooleanOperation operation, string name, Brush brush = null) {
            this.operation = operation;
            subTrees = new();
            this.Name = name;
            this.brush = brush;
        }

        public PolygonSide Categorize(ConvexPolygonGeometry polygon) {
            if (brush != null) {
                return brush.Categorize(polygon);
            }

            var compositeCategory = PolygonSide.OUTSIDE;

            foreach (var subtree in subTrees) {
                var subCategory = subtree.Categorize(polygon);
                switch (subtree.operation) {
                    case BooleanOperation.Add:
                        compositeCategory = CategorizationUtilities.AddCategorize(compositeCategory, subCategory);
                        break;
                    case BooleanOperation.Subtract:
                        compositeCategory = CategorizationUtilities.SubtractCategorize(compositeCategory, subCategory);
                        break;
                    case BooleanOperation.Intersect:
                        compositeCategory = CategorizationUtilities.IntersectCategorize(compositeCategory, subCategory);
                        break;
                }
            }

            return compositeCategory;

        }

        public List<Brush> GetBrushes() {
            var brushes = new List<Brush>();
            if (brush != null) {
                brushes.Add(brush);
            }

            foreach (var subtree in subTrees) {
                brushes.AddRange(subtree.GetBrushes());
            }

            return brushes;
        }

        public void AddTree(CSGTree subTree) {
            subTrees.Add(subTree);
        }

        public static string opStr(BooleanOperation? operation) {
            return operation switch {
                (BooleanOperation.Add) => "+ ",
                (BooleanOperation.Subtract) => "- ",
                (BooleanOperation.Intersect) => "∩ ",
                _ => "",
            };
        }

        public string Print(int level = 0) {
            var opName = Enum.GetName(typeof(BooleanOperation), operation);
            var lineStr = string.Join("", Enumerable.Range(0, level).Select((i) => "|>> ")) + opStr(operation) + Name;

            foreach (var tree in subTrees) {
                lineStr += "\n";
                lineStr += tree.Print(level + 1);
            }

            return lineStr;
        }
    }
}