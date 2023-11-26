
using UnityEngine;

namespace PolyArchitect.Core {
    public partial class Brush {

        public static Brush MakeCylinder(int sides, Transform transform) {
            var (planes, texDirs) = BrushBuilders.MakeCylinder(sides);
            return new Brush(planes, texDirs, Convert.Transform(transform));
        }

        public static Brush MakeBox(Transform transform) {
            var (planes, texDirs) = BrushBuilders.MakeRectangularCuboid(System.Numerics.Vector3.UnitY, System.Numerics.Vector3.UnitX);
            return new Brush(planes, texDirs, Convert.Transform(transform));
        }

        internal static Brush MakeCone(int sides, Transform transform) {
            var (planes, texDirs) = BrushBuilders.MakeCone(sides, System.Numerics.Vector3.UnitY, System.Numerics.Vector3.UnitX);
            return new Brush(planes, texDirs, Convert.Transform(transform));
        }
    }
}