extends Camera2D

signal camera_on_place()
# Declare member variables here. Examples:
# var a = 2
# var b = "text"
const want = -2450
var triggered = false
var temp = 0
var fight = false
# Called when the node enters the scene tree for the first time.
func _ready():
	emit_signal("camera_on_place")
	pass # Replace with function body.
	
func _process(delta):
	if (fight):
		return



var shake_amount = 3.0
func _on_CutScene_shake_camera():
	set_offset(Vector2( \
		rand_range(-1.0, 1.0) * shake_amount, \
		rand_range(-1.0, 1.0) * shake_amount \
	))


func _on_TriggerGroup1_trigger(x):
	limit_left = 1900 - 1280
	limit_right = 1900
	fight = true
	pass # Replace with function body.


func _on_Group1_end():
	limit_left = 0
	limit_right = 9311
	fight = false
	pass # Replace with function body.


func _on_TriggerGroup2_trigger(x):
	limit_left = 4000 - 1280
	limit_right = 4000
	fight = true


func _on_Group2_end():
	limit_left = 0
	limit_right = 9311
	fight = false


func _on_TriggerGroup4_trigger(x):
	limit_left = 6000 - 1280
	limit_right = 6000
	fight = true
	
func _on_Group3_end():
	limit_left = 0
	limit_right = 9311
	fight = false

func _on_Cutscene_area_entered(area):
	if (area.is_in_group("Player")):
		limit_right = 9311
		limit_left = 9311 - 1280






