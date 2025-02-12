using Godot;
using System;

/// <summary>
/// This class is for use in chris_workspace.tscn.
/// It displays a few beats and highlights the one that is currently playing.
/// Good for visualizing the rhythm.
/// </summary>
public partial class RhythmDebugUI : Control
{
	[Export] Conductor conductor;

	Label beat1;
	Label beat2;
	Label beat3;
	Label beat4;

	Label and;

	Label currentPhraseName;

	private Label[] beatLabels;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		beat1 = GetNode<Label>("1");
		beat2 = GetNode<Label>("2");
		beat3 = GetNode<Label>("3");
		beat4 = GetNode<Label>("4");

		and = GetNode<Label>("&");

		beatLabels = new Label[] { null, beat1, beat2, beat3, beat4 };

		currentPhraseName = GetNode<Label>("CurrentPhraseName");

		conductor.OnBeat += Beat;
		conductor.OnFinalBeat += ResetColors;
	}

	private void Beat(float beatIndex)
	{
		ColorOnBeat(beatIndex);

		currentPhraseName.Text = conductor.CurrentPhrase.Description;
	}

	/// <summary>
	/// Highlights the beat number that is currently playing.
	/// If it is inbetween it will highligh the &.
	/// </summary>
	/// <param name="beatThatIsOn"></param>
	private void ColorOnBeat(float beatThatIsOn)
	{
		if(beatThatIsOn % 1 == 0)
		{
			beatLabels[(int)beatThatIsOn].SelfModulate = Colors.Red;

			foreach (Label label in beatLabels)
			{
				if (label != null && label != beatLabels[(int)beatThatIsOn])
				{
					label.SelfModulate = Colors.White;
				}
			}

			and.SelfModulate = Colors.White;
		}
		else
		{
			and.SelfModulate = Colors.Red;
		}
	}

	/// <summary>
	/// Turns all the beat numbers white if it is off.
	/// </summary>
	private void ResetColors()
	{
		if(!conductor.IsPlaying)
		{
			foreach (Label label in beatLabels)
			{
				if(label != null)
				{
					label.SelfModulate = Colors.White;
				}
			}
		}
	}

	/// <summary>
	/// These are functions to rig up with button signals.
	/// </summary>
	public void PressPlay()
	{
		conductor.Play();
	}

	public void PressPause()
	{
		conductor.Pause();
	}
}
