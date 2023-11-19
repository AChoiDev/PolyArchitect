
namespace PolyArchitect.Core {
public partial class Brush {

    public static Brush MakeCylinder(int sides, UnityEngine.Transform transform) {
        var (planes, texDirs) = BrushBuilders.MakeCylinder(sides);
        return new Brush(planes, texDirs, Convert.Transform(transform));
    }

}
}