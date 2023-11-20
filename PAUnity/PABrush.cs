using PolyArchitect.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PolyArchitect.Unity {

    // component wrapper for core brush 
    [ExecuteInEditMode()]
    public class PABrush : MonoBehaviour {

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
                brush = Core.Brush.MakeCylinder(6, transform);
                preMatrixTransform = transform.localToWorldMatrix;
            }
        }

        public Brush GetBrush() {
            return brush;
        }

    }
}