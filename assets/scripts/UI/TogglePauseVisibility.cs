using Godot;
using System;

public partial class TogglePauseVisibility : CanvasLayer
{
    [Export]public bool visibleOnPause = true;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PauseManager.Instance.GamePauseToggle += ToggleVisibility;
	}
	public void ToggleVisibility(bool isPaused,bool playHit)
	{
        if (!playHit)
		{
			Show();
			return;
		}

		if(visibleOnPause==isPaused)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

}
