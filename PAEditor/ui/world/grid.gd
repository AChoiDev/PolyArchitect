@tool
extends MeshInstance3D


const GRID_SIZE: float = 32.0;
const GRID_SPACING: float = 1.0;
const GRID_EXTENT: float = GRID_SPACING * GRID_SIZE;
const GRID_START: float = -GRID_EXTENT / 2;
const GRID_END: float = GRID_EXTENT / 2;

const LINE_COLOR: Color = Color(0.2, 0.2, 0.2);
const MAJOR_LINE_COLOR: Color = Color(0.5, 0.5, 0.5);

const X_AXIS_LINE_COLOR: Color = Color(1.0, 0.0, 0.0);
const Y_AXIS_LINE_COLOR: Color = Color(0.0, 1.0, 0.0);
const Z_AXIS_LINE_COLOR: Color = Color(0.0, 0.0, 1.0);


func _ready() -> void:
	# https://github.com/godotengine/godot/issues/37016
	# BUG: The green vertical line representing the y-axis flickers when the viewport is an
	# even number of pixels wide, this is an ongoing issue as of me writing this and has some
	# potential manual fixes that could be implemented here. MSAA can fix this but doesn't work
	# for everyone.
	var grid_mesh = ImmediateMesh.new();
	
	# NOTE: The look of the grid can be cleaner by reordering how major and minor grid
	# lines are added to the surface in order to have some render on top of eachother
	# probably using render priorities, attempting to ignore depth would make the lines
	# draw on top of object in the scene so this is probably the only way?
	# This would go in the order of Minor->Major->Axis.
	
	# Grid
	grid_mesh.surface_begin(Mesh.PRIMITIVE_LINES);
	
	for i in range(GRID_SIZE + 1):
		var x = GRID_SPACING * i + GRID_START;
		var color = LINE_COLOR;
		if i % 8 == 0:
			color = MAJOR_LINE_COLOR;
		
		var start = Vector3(x, 0.0, GRID_START);
		var end = Vector3(x, 0.0, GRID_START + GRID_EXTENT);
		
		grid_mesh.surface_set_color(color);
		grid_mesh.surface_add_vertex(start);
		grid_mesh.surface_add_vertex(end);
	
	for i in range(GRID_SIZE + 1):
		var z = GRID_SPACING * i + GRID_START;
		var color = LINE_COLOR;
		
		var start = Vector3(GRID_START, 0.0, z);
		var end = Vector3(GRID_START + GRID_EXTENT, 0.0, z);
		if i % 8 == 0:
			color = MAJOR_LINE_COLOR;
		
		grid_mesh.surface_set_color(color);
		grid_mesh.surface_add_vertex(start);
		grid_mesh.surface_add_vertex(end);
	
	grid_mesh.surface_end();
	
	# X-Axis
	grid_mesh.surface_begin(Mesh.PRIMITIVE_LINES);
	grid_mesh.surface_set_color(X_AXIS_LINE_COLOR);
	grid_mesh.surface_add_vertex(Vector3(GRID_START, 0.0, 0.0));
	
	grid_mesh.surface_set_color(X_AXIS_LINE_COLOR);
	grid_mesh.surface_add_vertex(Vector3(GRID_END, 0.0, 0.0));
	grid_mesh.surface_end();
	
	# Y-Axis
	grid_mesh.surface_begin(Mesh.PRIMITIVE_LINES);
	grid_mesh.surface_set_color(Y_AXIS_LINE_COLOR);
	grid_mesh.surface_add_vertex(Vector3(0.0, GRID_START, 0.0));
	
	grid_mesh.surface_set_color(Y_AXIS_LINE_COLOR);
	grid_mesh.surface_add_vertex(Vector3(0.0, GRID_END, 0.0));
	grid_mesh.surface_end();
	
	# Z-Axis
	grid_mesh.surface_begin(Mesh.PRIMITIVE_LINES);
	grid_mesh.surface_set_color(Z_AXIS_LINE_COLOR);
	grid_mesh.surface_add_vertex(Vector3(0.0, 0.0, GRID_START));
	
	grid_mesh.surface_set_color(Z_AXIS_LINE_COLOR);
	grid_mesh.surface_add_vertex(Vector3(0.0, 0.0, GRID_END));
	grid_mesh.surface_end();
	
	mesh = grid_mesh;
