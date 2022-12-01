extends Timer


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (8.5 - get_node("../TimerStart").time_left >= 1.8 and !get_node("../TimerStart").is_stopped()):
		get_node("../CanvasLayer/PostProcessing_tool").vignette_show = true
		get_node("../CanvasLayer/PostProcessing_tool").vignette_scale = 0
	pass


func _on_Camera2D_off():
	start()
	pass # Replace with function body.


func _on_TimeOff_timeout():
	get_node("../AudioStreamPlayer").stream_paused = true
	get_node("../AudioStreamPlayer2").stream_paused = true
	get_node("../AudioStreamPlayer").playing = false
	get_node("../AudioStreamPlayer2").playing = false
	get_node("../Avaria").stream.loop = false
	get_node("../Avaria").play()
	get_node("../TimerStart").start()
	pass # Replace with function body.


func _on_TimerStart_timeout():
	get_tree().change_scene("res://Scenes/MainMenu.tscn")
	pass # Replace with function body.
