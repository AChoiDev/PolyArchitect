extends VBoxContainer


@onready var about_window: AcceptDialog = $AboutPopup;


func _ready() -> void:
	MenuBarBus.file_quit_pressed.connect(func():
		get_tree().quit();
	);
	
	MenuBarBus.help_about_pressed.connect(func():
		about_window.popup();
	);
