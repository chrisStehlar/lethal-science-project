using Godot;

public partial class Conductor : Node
{
	// Variables

	private double bpm = 120.0f; // bpm for the Conductor - will adjust to the bpm of the current phrase
	private int beatsPerMeasure = 4; // same as above
	private Pitch key;

	private Timer beatTimer; // time between each beat
	private int wholeBeatsThisMeasure = 1;
	private int beatSubdivisions = 0; // for eighth notes and beyond

	private MetronomePlayer clickTrack;
	[Export] public bool ClickTrackEnabled {get; set;} = true;
	[Export] public int AccentedBeat {get; set;} = 1;

	[Export] public bool PrintToConsoleEnabled {get; set;} = false; // for debugging

	// rule of thumb - dont go over 20 channels total

	[Export] public Phrase intro;
	[Export] public Phrase phrase;

	private Phrase currentPhrase;

	private bool pauseQueued = false;
	public bool IsPlaying {get; set;} = false;
	public int BeatsPerMeasure { get { return beatsPerMeasure; } }

	[Export] private AudioStreamPlayer rootChannel;

	/// <summary>
	/// The rate at which the beat will play.
	/// ex. 1 = every beat, 2 = every half beat, 4 = every quarter note.
	/// Do not set this directly.
	/// </summary>
	[Export] public int BeatRate {get; set;} = 1;
	private int queuedBeatRateChange = 0;

	public delegate void BeatEventHandler(float beat); // beats can be decimals (1/2 beat, 1/4 beat)
	public event BeatEventHandler OnBeat;

	public delegate void VoidEventHandler();
	public event VoidEventHandler OnFinalBeat;

	// Godot methods

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		clickTrack = GetNode<MetronomePlayer>("Metronome");

		currentPhrase = phrase;

		OnBeat += PrintBeat;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// P will toggle the conductor
		if(Input.IsActionJustPressed("P") && IsPlaying)
		{
			Pause();
		}
		else if(Input.IsActionJustPressed("P") && !IsPlaying)
		{
			Play(currentPhrase);
		}

		// up and down will change the beat rate - applied at end of measure
		if(Input.IsActionJustPressed("addBeat"))
		{
			IncrementBeatRate();
		}
		else if (Input.IsActionJustPressed("subtractBeat"))
		{
			DecrementBeatRate();
		}

		// pause will take effect at end of measure
		if(pauseQueued && wholeBeatsThisMeasure == 1)
		{
			beatTimer.Stop();
			IsPlaying = false;
			pauseQueued = false;

			PlayFinalBeat();
		}
	}

	// Methods

	/// <summary>
	/// Starting off point for the Conductor, call this to make things happen.
	/// </summary>
	public void Play(Phrase entryPhrase)
	{
		if(beatTimer == null)
		{
			beatTimer = new Timer();
			AddChild(beatTimer);
			beatTimer.Timeout += () => Beat(); // play another beat AFTER the last beat
		}
		beatTimer.Stop();

		SetConductorParameters(entryPhrase);
		rootChannel.Stream = entryPhrase.loop;

		UpdateBeatRate();
		Beat(); // start the first beat - this will play the next ones too

		IsPlaying = true;
	}

	private void UpdateBeatRate()
	{
		var secondsPerBeatEvent = (60.0 / bpm) * ((double)1/BeatRate);
		GD.Print("seconds per beat event: " + secondsPerBeatEvent);
		beatTimer.WaitTime = secondsPerBeatEvent;
		beatTimer.OneShot = true; // do not loop automatically
		wholeBeatsThisMeasure = 1; // start on 1st beat
		beatSubdivisions = 0;
	}

	/// <summary>
	/// Stops the beat timer, but will play out any more stems until they are done with their loop.
	/// </summary>
	public void Pause()
	{
		pauseQueued = true;
	}

	/// <summary>
	/// Public facing functions to queue up a new beat rate.
	/// </summary>
	public void IncrementBeatRate()
	{
		queuedBeatRateChange += 1;
	}

	public void DecrementBeatRate()
	{
		if(BeatRate - queuedBeatRateChange > 1)
		{
			queuedBeatRateChange -= 1;
		}
	}

	/// <summary>
	/// Plays the metronome sound. Accented beat will sound different than the others.
	/// </summary>
	private void PlayClickTrack()
	{
		if(wholeBeatsThisMeasure == AccentedBeat)
			clickTrack.PlayAccentedTick();
		else
			clickTrack.PlayTick();
	}

	/// <summary>
	/// Plays one more beat after the conductor is finished.
	/// This is the "first" beat of a measure that doesn't exist,
	/// but it serves a good purpose for UI and logic cleanup.
	/// </summary>
	private void PlayFinalBeat()
	{
		Timer finalBeatTimer = new Timer();
		AddChild(finalBeatTimer);
		finalBeatTimer.WaitTime = 60.0 / bpm;

		finalBeatTimer.OneShot = true;
		finalBeatTimer.Timeout += () => {
			OnFinalBeat?.Invoke();
			finalBeatTimer.QueueFree();
		};
		finalBeatTimer.Start();
	}

	/// <summary>
	/// Sets the conductor parameters to sync with the current phrase.
	/// </summary>
	/// <param name="phrase"></param>
	private void SetConductorParameters(Phrase phrase)
	{
		beatsPerMeasure = phrase.loop.BeatCount;
		bpm = phrase.loop.Bpm;
		key = phrase.Key;

		beatTimer.WaitTime = 60.0 / bpm;
	}

	/// <summary>
	/// Logic performed every beat. (Like Process() but for beats instead of frames.)
	/// </summary>
	private void Beat()
	{
		if (ClickTrackEnabled)
		{
			PlayClickTrack();
		}
		
		float beat = wholeBeatsThisMeasure + ((float)beatSubdivisions / BeatRate);
		OnBeat?.Invoke(beat);

		// start of new measure logic
		if(beat == 1)
		{

			if(queuedBeatRateChange != 0)
			{
				BeatRate += queuedBeatRateChange;
				queuedBeatRateChange = 0;
				UpdateBeatRate();
			}

			if(!pauseQueued)
			{
				rootChannel.Play();
			}
		}

		//  end of measure logic
		if(wholeBeatsThisMeasure == beatsPerMeasure && beatSubdivisions == BeatRate - 1)
		{
			wholeBeatsThisMeasure = 1;
			beatSubdivisions = 0;
		}
		// end of beat logic
		else
		{
			beatSubdivisions += 1;

			if(beatSubdivisions / BeatRate >= 1)
			{
				beatSubdivisions = 0;
				wholeBeatsThisMeasure += 1;
			}
		}

		beatTimer.Start();
	}

	/// <summary>
	/// Debug method tied to the OnBeat event.
	/// </summary>
	/// <param name="beat"></param>
	private void PrintBeat(float beat)
	{
		if(!PrintToConsoleEnabled) return;
		
		GD.Print("beat: " + beat);
		GD.Print("whole beats: " + wholeBeatsThisMeasure);
	}
}
