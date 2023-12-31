extends PopupMenu


enum {
	FILE_QUIT,
}


func _ready() -> void:
	self.id_pressed.connect(self._id_pressed);
	
	add_item("Quit", FILE_QUIT, KEY_Q | KEY_MASK_CTRL);


func _id_pressed(id: int) -> void:
	match id:
		FILE_QUIT:
			MenuBarBus.file_quit_pressed.emit();
