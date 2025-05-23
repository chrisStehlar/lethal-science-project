using Godot;
using System;

public partial class TogglePauseVisibility : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MenuManager.Instance.GamePauseToggle += ToggleVisibility;
	}

	private void ToggleVisibility()
	{
        Visible = !Visible;
	}
}
