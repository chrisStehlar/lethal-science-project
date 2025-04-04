extends Node

# Variables
@export var TrackName: String;
@export var ClickTrackEnabled: bool;
@export var PrintToConsoleEnabled: bool = false # for debugging

var IsPlaying:
	get:
		return root_channel.playing

@onready var root_channel: AudioStreamPlayer = $RhythmNotifier/AudioStreamPlayer
@onready var rhythmNotifier: RhythmNotifier = $RhythmNotifier
@onready var clickTrack = $"RhythmNotifier/Metronome"
var interactive_stream
var playback

# The rate at which the beat will play.
# Do not set this directly.
@export var BeatLength: float = 1

signal OnBeat(beat: int)

# Called when the node enters the scene tree for the first time.
func _ready():
	rhythmNotifier.beats(BeatLength).connect(Beat);
	interactive_stream = root_channel.stream as AudioStreamInteractive
	
	start()

# Always plays from the start of the song.
func start():
	root_channel.play();
	rhythmNotifier.running = true;

func Beat(beat: float):
	if (roundi(beat) % 4 == 0):
		if (ClickTrackEnabled): clickTrack.PlayTick();
	if (PrintToConsoleEnabled): print("beat: ", beat);
	OnBeat.emit(beat);

func stop():
	root_channel.stop();
	rhythmNotifier.running = false;
