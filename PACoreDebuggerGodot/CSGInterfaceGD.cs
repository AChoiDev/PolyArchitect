using System.Linq;
using PolyArchitect.Core;
using Vector3 = System.Numerics.Vector3;
using Plane = PolyArchitect.Core.Plane;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Numerics.Quaternion;

[Godot.Tool]
public partial class CSGInterfaceGD : Godot.Node
{
	[Godot.Export]
	private Godot.MeshInstance3D meshInstance3D;


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

	private void makemesh(MyMesh mesh) {

		var vertexQuery = from vert in mesh.vertices select new Godot.Vector3(vert.X, vert.Y, vert.Z);
		var vertices = vertexQuery.ToArray();
		var indices = mesh.indices.ToArray();
		for (int i = 0; i < indices.Length; i += 3)
		{
            (indices[i+2], indices[i]) = (indices[i], indices[i+2]);
        }
        var normalQuery = from normal in mesh.normals select new Godot.Vector3(normal.X, normal.Y, normal.Z);
		var normals = normalQuery.ToArray();
		

		// Initialize the ArrayMesh.
		var arrMesh = new Godot.ArrayMesh();
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Godot.Mesh.ArrayType.Max);
		arrays[(int)Godot.Mesh.ArrayType.Vertex] = vertices;
		arrays[(int)Godot.Mesh.ArrayType.Index] = indices;
		arrays[(int)Godot.Mesh.ArrayType.Normal] = normals;

		// Create the Mesh.
		arrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, arrays);
		meshInstance3D.Mesh = arrMesh;
    }

	[Godot.Export]
	private bool hasSetMesh = false;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		SetupDebugDraw();

		var myScene = new Scene();

		var planeList = BrushBuilders.MakeCylinder(4, Vector3.UnitY, Vector3.UnitZ).Item1;
		// var planeList = BrushBuilders.MakeRectangularCuboid(Vector3.UnitY, Vector3.UnitZ).Item1;
		var polyhedron = ConvexPolyhedron.Construct(planeList).Item1;
		
		var transformTwo = Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, 0f) * Matrix4x4.CreateTranslation(Vector3.Zero);
		var transformOne = Matrix4x4.CreateScale(new Vector3(1, 1, 2)) * Matrix4x4.CreateTranslation(new Vector3(1, 0, 0));
		var transformThree = Matrix4x4.CreateScale(new Vector3(2, 1, 2)) * Matrix4x4.CreateTranslation(new(0, -3.5f, 0));
		var transformLink = Matrix4x4.CreateScale(new Vector3(1, 1, 1)) * Matrix4x4.CreateTranslation(new Vector3(0, -5, 0));

		var csgLink = myScene.MakeNode(new CSGLink(BooleanOperation.Add));
		myScene.SetLocalTransform(csgLink, transformLink);

		var brushIDOne = myScene.MakeNode(new Brush(polyhedron, BooleanOperation.Add));
		myScene.SetParent(brushIDOne, csgLink);
		myScene.SetLocalTransform(brushIDOne, transformOne);

		var brushIDTwo = myScene.MakeNode(new Brush(polyhedron, BooleanOperation.Subtract));
		myScene.SetParent(brushIDTwo, csgLink);
		myScene.SetLocalTransform(brushIDTwo, transformTwo);

		var brushIDThree = myScene.MakeNode(new Brush(polyhedron, BooleanOperation.Intersect));
		myScene.SetLocalTransform(brushIDThree, transformThree);
		var result = myScene.GenerateMesh();

		if (hasSetMesh == false) {
			hasSetMesh = true;
			makemesh(result);
		}

		// makemesh(result);

		// myScen
	}
}
