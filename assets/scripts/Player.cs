using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 200.0f;

	[Export] AnimatedSprite2D sprites;

	//Absorption variables
	bool isAbsorbing;
	const float absorptionLength = 0.4f;
	//float absorptionTimer = absorptionLength;
	bool isOnCooldown;
	const float absorptionCooldown = 1f;
	//float cooldownTimer = absorptionCooldown;

	[Export]
	Timer absorptionTimer;
	[Export]
	Timer cooldownTimer;

	public bool IsAbsorbing
	{
		get { return isAbsorbing; }
	}

	public override void _Ready()
	{
		//sprites = GetChild<AnimatedSprite2D>(0);
		absorptionTimer.Timeout += () =>
		{
			isAbsorbing = false;
			Modulate = Color.FromHtml("FF0000");
			isOnCooldown = true;
			cooldownTimer.Start();
		};
		cooldownTimer.Timeout += () =>
		{
			Modulate = Color.FromHtml("FFFFFF");
			isOnCooldown = false;
		};
		sprites.Play();
	}
	
	public override void _PhysicsProcess(double delta)
	{

		// Get the input direction.
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		if (direction != Vector2.Zero)
		{
			Velocity = new Vector2(direction.X * Speed, direction.Y * Speed);
			Animate(direction);
		}
		else Velocity = Vector2.Zero;

		if (Input.IsActionJustPressed("absorb") && !isOnCooldown)
		{
			isAbsorbing = true;
            absorptionTimer.Start();
            Modulate = Color.FromHtml("FFFF00");
        }
		MoveAndSlide();
	}

	private void Animate(Vector2 direction)
	{
		if (direction.Y < 0)
		{
			sprites.Animation = "up";
            sprites.FlipH = false;
        }
		else if (direction.Y > 0)
		{
			sprites.Animation = "down";
            sprites.FlipH = false;
        }
		else if (direction.X > 0)
		{
			sprites.Animation = "side";
			sprites.FlipH = false;
		}
		else if (direction.X < 0)
		{
			sprites.Animation = "side";
			sprites.FlipH = true;
		}
	}
}
