extends AnimatedSprite


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

onready var globals = get_node("/root/Globals")
var activated = false
var end = false
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (end):
		get_node("../../../../CanvasLayer/PostProcessing_tool").vignette_scale -= 0.005
		if (get_node("../../../../CanvasLayer/PostProcessing_tool").vignette_scale < -0.1):
			globals.level3()
	pass


func _on_TriggerGroup3_trigger(x):
	play("default")
	get_node("../../../../CanvasLayer/UI").visible = false
	pass # Replace with function body.


func _on_AnimatedSprite_animation_finished():
	get_node("../../../../CanvasLayer/PostProcessing_tool").vignette_show = true
	end = true
	pass # Replace with function body.
