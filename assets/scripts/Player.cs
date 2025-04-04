using Godot;
using System;
using System.Linq.Expressions;

enum Direction
{
	Up, Down, Left, Right
}

public partial class Player : CharacterBody2D
{
	int maxHealth = 50;
	int currentHealth;
	float speed = 200.0f;

	
	AnimatedSprite2D sprites;
	Direction animDirection = Direction.Down;

	//status variables
	bool isAbsorbing;
	bool isOnCooldown;
	bool isDamaged;

	[Export] TextureProgressBar healthBar;
    Timer absorptionTimer;
	Timer cooldownTimer;
	Timer damageBuffer;
	double damageTime;

	[Export] AudioStreamWav damageSound;
	[Export] AudioStreamWav deathSound;
    AudioStreamPlayer soundPlayer;

    public bool IsAbsorbing
	{
		get { return isAbsorbing; }
	}
	public bool IsDamaged
	{
		get { return isDamaged; }
	}

	public int CurrentHealth
	{
		get { return currentHealth; }
	}

	[Signal]
	public delegate void GameOverEventHandler();

	//added a static instance to access signals wherever -Magnus
    public static Player Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;

    }
    public override void _Ready()
	{
		currentHealth = maxHealth;
		healthBar.MaxValue = maxHealth;
		UpdateHealthBar();

		sprites = GetChild<AnimatedSprite2D>(0);
		sprites.Play();
		
		soundPlayer = new AudioStreamPlayer();
		soundPlayer.Stream = damageSound;
        soundPlayer.VolumeDb -= 4;
        AddChild(soundPlayer);

		absorptionTimer = GameManager.Instance.CreateTimer(this, 0.4f, () =>
		{
			isAbsorbing = false;
			Modulate = Color.FromHtml("999999");
			isOnCooldown = true;
			cooldownTimer.Start();
		});
		cooldownTimer = GameManager.Instance.CreateTimer(this, 1, () =>
		{
			Modulate = Color.FromHtml("FFFFFF");
			isOnCooldown = false;
		});
		damageBuffer = GameManager.Instance.CreateTimer(this, 1.5f, () =>
		{
			Modulate = Color.FromHtml("FFFFFF");
			isDamaged = false;
		});
		
	}

	public override void _PhysicsProcess(double delta)
	{
		if (currentHealth <= 0) return;
		// Get the input direction.
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		if (direction != Vector2.Zero) { 
			Velocity = new Vector2(direction.X * speed, direction.Y * speed);
			SwitchDirection(direction);
		}
        else Velocity = Vector2.Zero;
        Animate(direction);

        if (Input.IsActionJustPressed("absorb") && !isOnCooldown && !isDamaged)
		{
			isAbsorbing = true;
            absorptionTimer.Start();
        }
		MoveAndSlide();
        
        if (isDamaged)
		{
			damageTime += delta;
			if (damageTime > 0.02)
			{
                Modulate = Modulate == Color.FromHtml("777777") ? Color.FromHtml("BBBBBB") : Color.FromHtml("777777");
				damageTime = 0;
            }
		}
        
    }

	public void Damage(int projectileDamage)
	{
		soundPlayer.Play();
		currentHealth -= projectileDamage;
		isDamaged = true;
		damageBuffer.Start();
		UpdateHealthBar();
    }

	public void Death(Action timeoutFunction)
	{
		Timer deathTimer = GameManager.Instance.CreateTimer(this, (float)deathSound.GetLength() - 2, timeoutFunction);
		soundPlayer.Stream = deathSound;
		soundPlayer.VolumeDb = 0;
		soundPlayer.Play();
		deathTimer.Start();
		sprites.Play("death");
	}

    #region Visuals and UI

    private void SwitchDirection(Vector2 direction)
	{
		if (direction.Y < 0)
			animDirection = Direction.Up;
		else if (direction.Y > 0)
			animDirection = Direction.Down;
		else if (direction.X > 0)
			animDirection = Direction.Right;
		else if (direction.X < 0)
			animDirection = Direction.Left;
		Animate(direction);
    }

	private void Animate(Vector2 direction)
	{
		string animDirection;
        sprites.FlipH = false;	
        switch (this.animDirection)
		{
			case Direction.Up:
				animDirection = "up";
				break;
			case Direction.Down:
				animDirection = "down";
				break;
			case Direction.Left:
				if (!isAbsorbing)
				{
					sprites.FlipH = true;
					goto default;
				}
				animDirection = "left";
				break;
			case Direction.Right:
				if (!isAbsorbing) goto default;
				animDirection = "right";
				break;
			default:
				animDirection = "side";
				break;
		}
		string absorb = isAbsorbing ? "absorb-" : "";
		string state = direction.X == 0 && direction.Y == 0 ? "idle" : "walk";
		sprites.Animation = absorb + state + '-' + animDirection;
		sprites.Play();
	}

	private void UpdateHealthBar()
	{
        Tween healthBarTween = CreateTween();
        healthBarTween.TweenProperty(healthBar, "value", currentHealth, 0.2).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        if (healthBar.Value < 0)
		{
			healthBar.Value = healthBar.MaxValue;
		}
	}

    #endregion
}
