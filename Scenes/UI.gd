extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

onready var player = get_node("/root/Globals")
var initial_hp
var initial_x
# Called when the node enters the scene tree for the first time.
func _ready():
	print(player.hp)
	initial_hp = player.hp
	initial_x = $HP.region_rect.position.x
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if (player.hp <= 0):
		return
	var percentage = (player.hp * 100) / initial_hp
	var x = (-2.45*percentage)+1170
	$HP.region_rect.position.x = x
	pass
