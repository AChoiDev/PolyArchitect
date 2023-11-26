using System.Collections.Generic;
using UnityEngine;

namespace PolyArchitect.Core {
    public partial class BrushBuilders {
        // TODO: make commented methods use oriented planes 
        // (see brush builders main file for more info)

        // public static () MakeRectangularCuboid() {
        // return MakeRectangularCuboid(Convert.Vector(Vector3.up), Convert.Vector(Vector3.forward));
        // }
        public static (List<Plane>, List<System.Numerics.Vector3>) MakeCylinder(int sides) {
            return MakeCylinder(sides, Convert.Vector(Vector3.up), Convert.Vector(Vector3.forward));
        }
        // public static ConvexPolyhedron MakeCone(int sides) {
        // return MakeCone(sides, Convert.Vector(Vector3.up), Convert.Vector(Vector3.forward));
        // }
    }
}