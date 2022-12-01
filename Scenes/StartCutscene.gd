extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
onready var globals = get_node("/root/Globals")
var on = false
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if ($CanvasLayer/PostProcessing_tool.vignette_scale < 0.9 and !on):
		$CanvasLayer/PostProcessing_tool.vignette_scale += 0.001
	elif (!on):
		on = true
		$AnimatedSprite.play("2")
		$CanvasLayer/PostProcessing_tool.vignette_scale = 0.56
		$AudioStreamPlayer.play()
		$Timer.start()
		
	if ($AnimatedSprite.animation == "2" and $AnimatedSprite.playing):
		$Camera2D.set_offset(Vector2( \
			rand_range(-1.0, 1.0) * 4.0, \
			rand_range(-1.0, 1.0) * 4.0 \
		))
		


func _on_Timer_timeout():
	$AnimatedSprite.stop()
	$CanvasLayer/PostProcessing_tool.vignette_scale = 0
	globals.level1()
	
