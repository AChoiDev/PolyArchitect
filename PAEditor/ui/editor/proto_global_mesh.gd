extends MeshInstance3D

var _interface: Node = null;

func _onMeshUpdate(arrMesh : ArrayMesh):
	set_mesh(arrMesh)
	print("mesh updated")

# Called when the node enters the scene tree for the first time.
func _ready():
	_interface = get_node("/root/PAWorkerInterface");
	_interface.Subscribe("GlobalMeshUpdated", _onMeshUpdate);


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
