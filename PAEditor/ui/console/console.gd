extends PanelContainer

const CURRENT_COMMAND_INDEX: int = -1;
const MAX_COMMAND_HISTORY: int = 512;


@onready var log_label: RichTextLabel = %Log;
@onready var command_line_edit: LineEdit = %CommandLineEdit;


var _current_command: String = "";
var _command_history: Array = [];
var _current_history_index: int = CURRENT_COMMAND_INDEX;
var _interface: Node = null;

func _onConnect() -> void:
	log_label.add_text("Connected to PA-Worker" + "\n");

func _pong(x) -> void:
	log_label.add_text(x + "\n");
func _sceneSaved(x) -> void:
	log_label.add_text("Scene Saved: " + x + "\n")
func _sceneAvailable(id, availability) -> void:
	if availability:
		log_label.add_text("Scene Now Available: " + id + "\n")
	else: 
		log_label.add_text("Scene Now Unavailable: " + id + "\n")

func _ready() -> void:
	_interface = get_node("/root/PAWorkerInterface");
	_interface.Subscribe("OnConnect", _onConnect);
	_interface.Subscribe("Pong", _pong);
	_interface.Subscribe("SceneSaved", _sceneSaved);
	_interface.Subscribe("SceneAvailable", _sceneAvailable);

func _next() -> void:
	if _current_history_index == CURRENT_COMMAND_INDEX:
		return;
	
	if _current_history_index == _command_history.size() - 1:
		_current_history_index = CURRENT_COMMAND_INDEX;
	else:
		_current_history_index += 1;
	
	_set_command();


func _previous() -> void:
	if _current_history_index == CURRENT_COMMAND_INDEX:
		_current_history_index = _command_history.size() - 1;
	elif _current_history_index != 0:
		_current_history_index -= 1;
	
	_set_command();


func _set_command() -> void:
	var command = _get_command();
	
	command_line_edit.set_block_signals(true);
	command_line_edit.text = command;
	command_line_edit.caret_column = command.length();
	command_line_edit.set_block_signals(false);


func _get_command() -> String:
	if _current_history_index == CURRENT_COMMAND_INDEX:
		return _current_command;
	else:
		return _command_history[_current_history_index];


func _on_command_line_edit_text_changed(new_text: String) -> void:
	_current_command = new_text;


func _on_command_line_edit_text_submitted(new_text: String) -> void:
	if new_text.is_empty():
		return;
	
	command_line_edit.clear();
	_command_history.push_back(new_text);
	
	if _command_history.size() > MAX_COMMAND_HISTORY:
		_command_history.pop_front();
	
	log_label.add_text(new_text + "\n");
	
	_current_history_index = CURRENT_COMMAND_INDEX;
	_current_command = "";

	if new_text == "ping":
		_interface.Request("Ping", []);
	elif new_text == "save_scene":
		_interface.Request("SaveScene", ["sceneIDFake", "pathFake"]);
	elif new_text == "create_scene":
		_interface.Request("CreateScene", [])


func _on_command_line_edit_gui_input(event: InputEvent) -> void:
	if event is InputEventKey:
		if event.keycode == KEY_UP:
			if event.pressed:
				_previous();
			accept_event();
		elif event.keycode == KEY_DOWN:
			if event.pressed:
				_next();
			accept_event();
