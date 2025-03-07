using Godot;
using System;

public partial class InteractiveAudioTester : AudioStreamPlayer
{

	[Export] AudioStreamPlayer player;
	[Export] public Label label;
	AudioStreamInteractive stream;

	AudioStreamOggVorbis currentClip;
	AudioStreamPlaybackInteractive playback;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//stream = GetNode<AudioStreamInteractive>("AudioStreamPla");
		stream = player.Stream as AudioStreamInteractive;

		playback = GetStreamPlayback() as AudioStreamPlaybackInteractive;
		currentClip = stream.GetClipStream(playback.GetCurrentClipIndex()) as AudioStreamOggVorbis;
		GD.Print(currentClip);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//label.Text = stream();
		currentClip = stream.GetClipStream(playback.GetCurrentClipIndex()) as AudioStreamOggVorbis;

		label.Text = "clip index " + playback.GetCurrentClipIndex();
		label.Text += "\n" + "bpm: " + currentClip.Bpm;
		label.Text += "\n" + "bar beats: " + currentClip.BarBeats;
	}
}
