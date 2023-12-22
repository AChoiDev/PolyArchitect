using Vector3 = System.Numerics.Vector3;
using Plane = PolyArchitect.Core.Plane;
public static class PenDebug {
    private static System.Action<Vector3, Vector3> dynamicDrawLineFunc = (a, b) => {};
    private static System.Action<Vector3> dynamicDrawPointFunc = (a) => {};
    // private static System.Action<Plane> dynamicDrawPlaneFunc = (a) => {};

    public static void SetDrawLineFunc<T>(System.Action<T, T> lineDrawFunc, System.Func<Vector3, T> vectorConversionFunc) {
        dynamicDrawLineFunc = (a, b) => {
            lineDrawFunc.Invoke(vectorConversionFunc(a), vectorConversionFunc(b));
        };
    }

    public static void SetDrawPointFunc<T>(System.Action<T> pointDrawFunc, System.Func<Vector3, T> vectorConversionFunc) {
        dynamicDrawPointFunc = (a) => {
            pointDrawFunc.Invoke(vectorConversionFunc(a));
        };
    }
    // public static void SetDrawPlaneFunc<T>(System.Action<T> planeDrawFunc, System.Func<Plane, T> planeConversionFunc) {
    //     dynamicDrawPlaneFunc = (a) => {
    //         planeDrawFunc.Invoke(planeConversionFunc(a));
    //     };
    // }

    public static void DrawLine(Vector3 a, Vector3 b) => dynamicDrawLineFunc.Invoke(a, b);
    public static void DrawPoint(Vector3 a) => dynamicDrawPointFunc.Invoke(a);
    // public static void DrawPlane(Plane a) => dynamicDrawPlaneFunc.Invoke(a);


    private static System.Action<string> dynamicLogFunc = (a) => {};

    public static void SetLogFunc(System.Action<string> logFunc) {
        dynamicLogFunc = logFunc;
    }
    public static void Log(object obj) => dynamicLogFunc.Invoke(obj.ToString() ?? "null");

}