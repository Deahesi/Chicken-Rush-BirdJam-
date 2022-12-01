extends AnimatedSprite


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var end = false
signal cutscene_started()
signal cutscene_stopped()
signal shake_camera()
signal sound()
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (frame >= 12 and frame <= 16):
		emit_signal("shake_camera")
	if (frame == 2 or frame == 8 or frame == 12 or frame == 14):
		emit_signal("sound")


func _on_Camera2D_camera_on_place():
	play("default")
	emit_signal("cutscene_started")
	pass # Replace with function body.


func _on_AnimatedSprite_animation_finished():
	play("stopped")
	if (!end):
		emit_signal("cutscene_stopped")
		end = true
	pass # Replace with function body.
