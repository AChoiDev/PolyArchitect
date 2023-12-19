extends PopupMenu


enum {
	HELP_ABOUT,
}


func _ready() -> void:
	self.id_pressed.connect(self._id_pressed);
	
	add_item("About", HELP_ABOUT);


func _id_pressed(id: int) -> void:
	match id:
		HELP_ABOUT:
			MenuBarBus.help_about_pressed.emit();
