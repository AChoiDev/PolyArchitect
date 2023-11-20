using PolyArchitect.Core;
using UnityEngine;

namespace PolyArchitect.Unity {

    // component wrapper for core brush 
    [ExecuteInEditMode()]
    public class PABrush : MonoBehaviour {
        // NOTE: Temporary enum to control the type of brush generated
        public enum BrushType {
            Cylinder,
            Box,
            Cone,
        }

        // NOTE: Temporary variable to control the type of brush generated
        public BrushType type;

        public BooleanOperation operation;
        private Core.Brush brush = null;

        private Matrix4x4? preMatrixTransform = null;


        public void OnDrawGizmos() {
            if (brush != null) {
                if (transform.localToWorldMatrix != preMatrixTransform) {
                    brush.ChangeTransform(Convert.Transform(transform));
                    preMatrixTransform = transform.localToWorldMatrix;
                }
            } else {
                switch (type) {
                    case BrushType.Cylinder:
                        brush = Core.Brush.MakeCylinder(6, transform);
                        break;
                    case BrushType.Box:
                        brush = Core.Brush.MakeBox(transform);
                        break;
                    case BrushType.Cone:
                        brush = Core.Brush.MakeCone(6, transform);
                        break;
                    default:
                        break;
                }
                preMatrixTransform = transform.localToWorldMatrix;
            }
        }

        public Brush GetBrush() {
            return brush;
        }

    }
}