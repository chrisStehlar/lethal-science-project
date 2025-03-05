using Godot;
using System;

public partial class Enemy : StaticBody2D
{
	[Export] PackedScene projectileScene;
	[Export] Conductor conductor;

	int calmMax = 50;
	int calmCurrent = 0;
	[Export] TextureProgressBar calmMeter;

	[Export] Color projectileColor;
	[Export] int projectileSpeed;
	[Export] string shootingGuide = "";
	[Export] int guidePhrase;

	int guideLength;
	int currentMeasure = 0;

	AnimatedSprite2D sprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = GetChild<AnimatedSprite2D>(0);
		if (calmMeter != null)
		{
			calmMeter.MaxValue = calmMax;
			calmMeter.TintOver = projectileColor;
		}

		conductor.OnBeat += Beat;
		ResetGuide();
		sprite.AnimationFinished += () => { sprite.Animation = "idle"; };
	}

	/// <summary>
	/// Instead of _Process() let's use Beat() to run logic every beat.
	/// </summary>
	private void Beat(float beatIndex)
	{
		if (string.IsNullOrEmpty(shootingGuide))
		{
			SpawnProjectile();
			return;
		}
		if (conductor.PrintToConsoleEnabled) {
			GD.Print("Current Measure: " + currentMeasure);
			GD.Print("Current Note: " + CalculateGuideIndex(beatIndex));
		}
		if (shootingGuide[CalculateGuideIndex(beatIndex)] == '1')
			SpawnProjectile();
		if (beatIndex * conductor.BeatRate > guideLength) ResetGuide();
    }

	private void SpawnProjectile()
	{
		Projectile projectile = projectileScene.Instantiate() as Projectile;
		projectile.GlowColor = projectileColor;
		projectile.Speed = projectileSpeed;
		projectile.Orientation = GD.Randf() * 1.4f - 0.7f;
		AddChild(projectile);
		sprite.Animation = "swing";
		sprite.Play();
	}

	public void ResetGuide()
	{
        guideLength = shootingGuide.Length;
		currentMeasure = 0;
	}

	private int CalculateGuideIndex(float beatIndex)
	{
		int index = Mathf.FloorToInt(beatIndex * conductor.BeatRate - conductor.BeatRate);
		while (index > guideLength - 1){
			index -= guideLength;
		}
		return index;
	}

	public virtual void Pacify()
	{
		calmCurrent += 5;
		UpdateCalmness();
	}

	private void UpdateCalmness()
	{
        calmMeter.Value = calmCurrent;
    }
}
