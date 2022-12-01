extends Camera2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

signal off()

var zooming = false
var on = false
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (!zooming):
		return
	zoom.x -= 0.001
	zoom.y -= 0.001
	pass


func _on_Boss_Dead():
	zooming = true
	emit_signal("off")
	pass # Replace with function body.
