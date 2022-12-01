extends Node2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
export var texture1: StreamTexture
export var texture2: StreamTexture
export var texture3: StreamTexture
export var texture4: StreamTexture
export var texture5: StreamTexture
export var texture6: StreamTexture

var set1 = false
var set2 = true
var rng = RandomNumberGenerator.new()
# Called when the node enters the scene tree for the first time.
func _ready():
	rng.randomize()
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if ($Road1.global_position.x >= 2400 and !set1):
		$Road2.global_position.x = 500
		var n = rng.randi_range(-5, 6)
		if (n <= 0):
			$Road1.play("1")
		else:
			$Road1.play(str(n))
		set1 = true
		set2 = false
	if ($Road2.global_position.x >= 2400 and !set2):
		$Road2.global_position.x = 500
		var n = rng.randi_range(-3, 6)
		if (n <= 0):
			$Road2.play("1")
		else:
			$Road2.play(str(n))
		set2 = true
		set1 = false
		
	$Road1.position.x += 2000 * delta
	$Road2.position.x += 2000 * delta
