extends SubViewportContainer


func _input(event: InputEvent) -> void:
	if event is InputEventKey:
		if event.keycode == KEY_UP:
			accept_event();
		elif event.keycode == KEY_RIGHT:
			accept_event();
		elif event.keycode == KEY_DOWN:
			accept_event();
		elif event.keycode == KEY_LEFT:
			accept_event();
