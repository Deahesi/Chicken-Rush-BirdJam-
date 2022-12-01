extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var activated = false
var end = false
onready var globals = get_node("/root/Globals")
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (end):
		get_node("../../CanvasLayer/PostProcessing_tool").vignette_scale -= 0.005
		if (get_node("../../CanvasLayer/PostProcessing_tool").vignette_scale < -0.1):
			globals.level2()
	pass


func _on_TriggerGroup4_trigger(x):
	get_node("../../CanvasLayer/PostProcessing_tool").vignette_show = true
	get_node("../../Objects/Player/Player/Player").cutscene = true
	end = true
	pass # Replace with function body.
