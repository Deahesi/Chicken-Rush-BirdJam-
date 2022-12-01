extends Camera2D

signal camera_on_place()
# Declare member variables here. Examples:
# var a = 2
# var b = "text"
export var dev = true
var interVector = Vector2(-157, 0)
const want = -2450
var triggered = false
var temp = 0
var fight = false
var scene = true
# Called when the node enters the scene tree for the first time.
func _ready():
	interVector.x = position.x
	position.x = want
	if (dev):
		position.x = interVector.x
		emit_signal("camera_on_place")
	pass # Replace with function body.
	
func _process(delta):
	if (fight):
		return
	
	if (position.x < interVector.x):
		position.x += 8
	elif (position.x > interVector.x and position.x - interVector.x > 8 and get_node("../TimerCutscene").is_stopped()):
		position.x  -= 8
	elif (limit_left != 0):
		scene = false
		emit_signal("camera_on_place")
		limit_left = 0
	
	if (position.x >= interVector.x and triggered):
		get_node("../TimerCutscene").start()
		interVector.x = temp
		triggered = false


func _on_AnimatedSprite_cutscene_stopped():
	print_debug("GGGGFGG")
	interVector.x = 0


func _on_StTriggerCamera_trigger(x):
	temp = position.x
	interVector.x = x
	triggered = true
	pass # Replace with function body.

var shake_amount = 3.0
func _on_CutScene_shake_camera():
	set_offset(Vector2( \
		rand_range(-1.0, 1.0) * shake_amount, \
		rand_range(-1.0, 1.0) * shake_amount \
	))


func _on_TriggerGroup1_trigger(x):
	get_node("../TimerCutscene").stop()
	interVector.x = temp
	position.x = interVector.x
	triggered = false
	limit_left = 3500 - 1280
	limit_right = 3500
	fight = true
	pass # Replace with function body.


func _on_Group1_end():
	limit_left = 0
	limit_right = 12994
	fight = false
	pass # Replace with function body.


func _on_TriggerGroup2_trigger(x):
	limit_left = 6500 - 1280
	limit_right = 6500
	fight = true


func _on_Group2_end():
	limit_left = 0
	limit_right = 12994
	fight = false


func _on_TriggerGroup3_trigger(x):
	limit_left = 8900 - 1280
	limit_right = 8900
	fight = true


func _on_Group3_end():
	limit_left = 0
	limit_right = 12994
	fight = false


func _on_TriggerGroup5_trigger(x):
	limit_left = 11000 - 1280
	limit_right = 11000
	fight = true
	
func _on_Group4_end():
	limit_left = 0
	limit_right = 12994
	fight = false


func _on_Stairs_area_entered(area):
	if (area.is_in_group("Player") and !scene):
		limit_right = 12994
		limit_left = 12994 - 1280
		fight = true






