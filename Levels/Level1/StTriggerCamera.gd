extends Area2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var entered = false
signal trigger(x)
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass


func _on_StTriggerCamera_area_entered(area):
	if (area.is_in_group("Player") and !entered):
		emit_signal("trigger", 1800)
		entered = true
	pass # Replace with function body.
