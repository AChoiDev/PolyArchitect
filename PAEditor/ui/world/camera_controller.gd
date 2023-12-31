extends Node3D


@onready var camera: Camera3D = $Camera3D;
@onready var viewport = $"../../../../Viewport"; # NOTE: I hate this but it works for now

@export var arrow_look_speed = 1.0;

var pitch: float = PI / 4.0;
var yaw: float = PI / 4.0;


func _process(delta: float) -> void:
	# NOTE: Quick fix to ensure the only when the viewport is focused the camera will move
	if not viewport.has_focus():
		return;
	
	var vertical_look = 0.0;
	if Input.is_key_pressed(KEY_UP):
		vertical_look += 1.0;
	if Input.is_key_pressed(KEY_DOWN):
		vertical_look -= 1.0;
	
	var horizontal_look = 0.0;
	if Input.is_key_pressed(KEY_RIGHT):
		horizontal_look += 1.0;
	if Input.is_key_pressed(KEY_LEFT):
		horizontal_look -= 1.0;
	
	pitch += vertical_look * delta * arrow_look_speed;
	pitch = clampf(pitch, -PI / 2.0 + 0.01, PI / 2.0 - 0.01);
	
	yaw += horizontal_look * delta * arrow_look_speed;
	yaw = fmod(yaw, PI * 2.0);
	
	
	rotation.y = yaw;
	rotation.x = -pitch;
