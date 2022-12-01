extends Area2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var entered = false
export var group = "Group1"
signal trigger(x)
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass


func area_entered(area):
	if (area.is_in_group("Player") and !entered):
		print("TRIGGER")
		get_parent().get_parent().get_node("Objects").get_node(group).set("activated", true)
		entered = true
		emit_signal("trigger", 1)
