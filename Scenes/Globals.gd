extends Node


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
export var hp = 1000.0

func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
func level1():
	hp = 1000.0
	get_tree().change_scene("res://Levels/Level1/Level1.tscn")

func level2():
	hp = 1000.0
	get_tree().change_scene("res://Levels/Level2/Level2.tscn")

func level3():
	hp = 1000.0
	get_tree().change_scene("res://Levels/Level3/Level3.tscn")
	
func reload():
	hp = 1000.0
	get_tree().reload_current_scene()
