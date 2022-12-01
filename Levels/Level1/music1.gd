extends AudioStreamPlayer


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

onready var settings = get_node("/root/MainMenu")
var stopped = false
# Called when the node enters the scene tree for the first time.
func _ready():
	stream_paused = !settings.sound_on
	volume_db += settings.volume
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
func _physics_process(delta):
	stream_paused = !settings.sound_on
	if (!settings.sound_on):
		stop()
	elif (!playing and stopped):
		play()

func _on_CutScene_cutscene_stopped():
	playing = true
	stopped = true
	pass # Replace with function body.
