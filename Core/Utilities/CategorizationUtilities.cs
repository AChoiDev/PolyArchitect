
using System.Collections.Generic;

namespace PolyArchitect.Core {
public enum PointSide {OUTSIDE, ALIGNED, INSIDE}
public enum PolygonSide {OUTSIDE, ALIGNED, REVERSE_ALIGNED, INSIDE}

// TODO: rename this to polygon side utilities
// also investigate just using integers only instead of enums
public static class CategorizationUtilities {
    public const int IN = 0; // inside
    public const int ALN = 1; // aligned
    public const int RA = 2; // reverse aligned
    public const int OUT = 3; // outside
    private readonly static Dictionary<(int, int), int> addLookup = new(){
        // (a, b) => c
        { (IN, IN),     IN },
        { (IN, ALN),    IN },
        { (IN, RA),     IN },
        { (IN, OUT),    IN },

        { (ALN, IN),    IN },
        { (ALN, ALN),   ALN },
        { (ALN, RA),    IN },
        { (ALN, OUT),   ALN },

        { (RA, IN),     IN },
        { (RA, ALN),    IN },
        { (RA, RA),     RA },
        { (RA, OUT),    RA },

        { (OUT, IN),    IN },
        { (OUT, ALN),   ALN },
        { (OUT, RA),    RA },
        { (OUT, OUT),   OUT },
    };
    private readonly static Dictionary<(int, int), int> subtractLookup = new(){
        // (a, b) => c
        { (IN, IN),     OUT },
        { (IN, ALN),    RA },
        { (IN, RA),     ALN },
        { (IN, OUT),    IN },

        { (ALN, IN),    OUT },
        { (ALN, ALN),   OUT },
        { (ALN, RA),    ALN },
        { (ALN, OUT),   ALN },

        { (RA, IN),     OUT },
        { (RA, ALN),    RA },
        { (RA, RA),     OUT },
        { (RA, OUT),    RA },

        { (OUT, IN),    OUT },
        { (OUT, ALN),   OUT },
        { (OUT, RA),    OUT },
        { (OUT, OUT),   OUT },
    };
    private readonly static Dictionary<(int, int), int> intersectLookup = new(){
        // (a, b) => c
        { (IN, IN),     IN },
        { (IN, ALN),    ALN },
        { (IN, RA),     RA },
        { (IN, OUT),    OUT },

        { (ALN, IN),    ALN },
        { (ALN, ALN),   ALN },
        { (ALN, RA),    OUT },
        { (ALN, OUT),   OUT },

        { (RA, IN),     RA },
        { (RA, ALN),    OUT },
        { (RA, RA),     RA },
        { (RA, OUT),    OUT },

        { (OUT, IN),    OUT },
        { (OUT, ALN),   OUT },
        { (OUT, RA),    OUT },
        { (OUT, OUT),   OUT },
    };

    private readonly static Dictionary<PolygonSide, int> sideEnumToConstant = new(){
        {PolygonSide.ALIGNED, ALN},
        {PolygonSide.REVERSE_ALIGNED, RA},
        {PolygonSide.INSIDE, IN},
        {PolygonSide.OUTSIDE, OUT}
    };
    private readonly static Dictionary<int, PolygonSide> constantToSideEnum = new(){
        {ALN, PolygonSide.ALIGNED},
        {RA, PolygonSide.REVERSE_ALIGNED},
        {IN, PolygonSide.INSIDE},
        {OUT, PolygonSide.OUTSIDE}
    };


    public static PolygonSide AddCategorize(PolygonSide lh, PolygonSide rh) {
        var result = addLookup[(sideEnumToConstant[lh], sideEnumToConstant[rh])];
        return constantToSideEnum[result];
    }

    public static PolygonSide SubtractCategorize(PolygonSide lh, PolygonSide rh) {
        var result = subtractLookup[(sideEnumToConstant[lh], sideEnumToConstant[rh])];
        return constantToSideEnum[result];
    }

    public static PolygonSide IntersectCategorize(PolygonSide lh, PolygonSide rh) {
        var result = intersectLookup[(sideEnumToConstant[lh], sideEnumToConstant[rh])];
        return constantToSideEnum[result];
    }
}
}