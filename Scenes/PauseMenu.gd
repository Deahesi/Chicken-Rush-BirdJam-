extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
export var moff: StreamTexture
export var mon: StreamTexture
onready var globals = get_node("/root/Globals")

func _ready():
	visible = false
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (Input.is_action_just_pressed("ui_cancel") and !get_tree().paused):
		visible = true
		get_tree().paused = true
	elif (Input.is_action_just_pressed("ui_cancel")):
		visible = false
		get_tree().paused = false
	pass


func _on_TextureButton_toggled(button_pressed):
	print(button_pressed)
	var settings = get_node("/root/MainMenu")
	settings.sound_on = !button_pressed
	(get_node("TextureButton") as TextureButton).set_normal_texture(moff if button_pressed else mon)
	pass # Replace with function body.
