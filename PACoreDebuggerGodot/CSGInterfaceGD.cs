using PolyArchitect.Core;
using Vector3 = System.Numerics.Vector3;
using Plane = PolyArchitect.Core.Plane;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Numerics.Quaternion;

[Godot.Tool]
public partial class CSGInterfaceGD : Godot.Node
{


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	private void SetupDebugDraw() {
        static Godot.Vector3 vectorConvert(Vector3 a) {
			return new(a.X, a.Y, a.Z);
		}
        // static Godot.Plane planeConvert(Plane a) => new(vectorConvert(a.Normal), a.Displacement);

        PenDebug.SetDrawLineFunc((a, b) => DebugDraw3D.DrawLine(a, b), vectorConvert);
		PenDebug.SetDrawPointFunc((a) => DebugDraw3D.DrawPoints([a]), vectorConvert);
		PenDebug.SetLogFunc((a) => Godot.GD.Print(a));
		// PenDebug.SetDrawPlaneFunc((a) => DebugDraw3D.DrawGrid(a.), planeConvert)
		// GD.Print(PolyArchitect.CoreAPI.MAX_SCENES_LOADED_LIMIT);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		SetupDebugDraw();

		var myScene = new Scene();

		var planeList = BrushBuilders.MakeCylinder(5, Vector3.UnitY, Vector3.UnitZ).Item1;
		
		var transformOne = Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, 0f) * Matrix4x4.CreateTranslation(Vector3.One);
		var transformTwo = Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, Godot.Mathf.Pi * 0.5f) * Matrix4x4.CreateTranslation(Vector3.One * 1.8f);

		var brushIDOne = myScene.MakeNode(new Brush(planeList, transformOne, BooleanOperation.Add));
		var brushIDTwo = myScene.MakeNode(new Brush(planeList, transformTwo, BooleanOperation.Subtract));
		myScene.SliceBrushes();
		myScene.CategorizeBrushSplitPolygons();
		// myScen
	}
}
