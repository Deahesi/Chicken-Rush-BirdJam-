extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

var sound_on = true
var volume = 20
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass


func _on_TextureButton_pressed():
	get_tree().change_scene("res://Scenes/StartCutscene.tscn")
	pass # Replace with function body.
