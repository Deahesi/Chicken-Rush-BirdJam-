extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

export var activated = false
export var visible_g = false
signal end()

# Called when the node enters the scene tree for the first time.
func _process(delta):
	var children = get_children()
	var dead = 0
	if (get_child_count() == 0):
		queue_free()
	for child in children:
		var layer = child as CanvasLayer
		if (!is_instance_valid(layer)):
			continue
		if (layer.get_child_count() == 0):
			continue

		layer.get_child(0).set("enabled", activated)
		if (!visible_g):
			layer.visible = activated
		else:
			layer.visible = true
			
		if (layer.get_child_count() > 1):
			var death = false
			for c in layer.get_children():
				if c.death:
					death = true
			if (death):
				dead = dead + 1
		elif layer.get_child(0).death:
			dead = dead + 1
		
		
	if (dead == get_child_count()):
		emit_signal("end")
	pass
