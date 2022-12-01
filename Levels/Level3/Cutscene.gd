extends AnimatedSprite


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

signal sound()
signal sound1()
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if ($"../SceneTimer".is_stopped()):
		play("default")
		if (frame == 0):
			emit_signal("sound1")
		if (frame == 6):
			emit_signal("sound")
		if (frame >= 8):
			$"../AnimatedSprite".play("def2")


func _on_Cutscene_animation_finished():
	$"../../Objects/Boss/Boss/Boss".cutscene = false
	visible = false
	pass # Replace with function body.
