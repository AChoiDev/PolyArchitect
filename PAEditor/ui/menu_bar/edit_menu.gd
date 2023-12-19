extends PopupMenu


enum {
	
}


func _ready() -> void:
	self.id_pressed.connect(self._id_pressed);


func _id_pressed(id: int) -> void:
	pass;
