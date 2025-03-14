using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class Enemy : StaticBody2D
{
	[Export] string enemyName;
	[Export] PackedScene projectileScene;
	[Export] Conductor conductor;

	int calmMax = 50;
	int calmCurrent = 0;
	[Export] TextureProgressBar calmMeter;

	[Export] Color projectileColor;
	[Export] int projectileSpeed;

	List<string> shootingGuides = new List<string>();
    string currentShootingGuide;
	int currentClipIndex = 0;
	int currentGuideIndex = 0;

    int guideLength;
	int currentMeasure = 0;

	AnimatedSprite2D sprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadShootingGuide();
		sprite = GetChild<AnimatedSprite2D>(0);
		if (calmMeter != null)
		{
			calmMeter.MaxValue = calmMax;
			calmMeter.TintOver = projectileColor;
		}
		conductor.OnBeat += Beat;
		conductor.OnBeatRateChanged += ResetGuide;
		ResetGuide();
		sprite.AnimationFinished += () => { sprite.Animation = "idle"; };
		currentShootingGuide = shootingGuides[0];
	}

	/// <summary>
	/// Instead of _Process() let's use Beat() to run logic every beat.
	/// </summary>
	private void Beat(float beatIndex)
	{
		// Contingencies
		if (!conductor.IsPlaying) return;
		if (currentClipIndex != conductor.GetCurrentClipIndex())
		{
			ResetGuide();
		}
		if (string.IsNullOrEmpty(currentShootingGuide))
		{
			SpawnProjectile();
			return;
		}
		if (conductor.PrintToConsoleEnabled) {
			GD.Print("Current Measure: " + currentMeasure);
			GD.Print("Current Note: " + CalculateGuideIndex(beatIndex));
		}



		int guideNumber = currentShootingGuide[CalculateGuideIndex(beatIndex)] - '0';
		if (guideNumber == 1)
			SpawnProjectile();
		else if (guideNumber > 0)
		{
			for (int i = 0; i < guideNumber; i++)
				SpawnProjectile();
		}

		if (beatIndex * conductor.BeatRate > guideLength) ResetGuide();
        // If we're near the end of a measure
		if (beatIndex == conductor.BeatsPerMeasure + 1.0f / conductor.BeatRate)
        {
			
            if (currentGuideIndex < guideLength - 1) currentMeasure++;
            else currentMeasure = 0;
        }
    }

    #region Shooting Guide Functions
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
        currentClipIndex = conductor.GetCurrentClipIndex();
        currentShootingGuide = shootingGuides[currentClipIndex];
        guideLength = currentShootingGuide.Length;
		currentMeasure = 0;
	}

	private int CalculateGuideIndex(float beatIndex)
	{
		int index = Mathf.FloorToInt(beatIndex * conductor.BeatRate - conductor.BeatRate + conductor.BeatsPerMeasure * conductor.BeatRate * currentMeasure);
		while (index > guideLength - 1){
			index -= guideLength;
		}
		return index;
	}
    private void LoadShootingGuide()
    {
        string filePath = "res://guides/" + conductor.TrackName + "/" + enemyName + ".csv";

        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        string fileContent = file.GetAsText();
        string[] fileContentArray = fileContent.Split(',');
        for (int i = 0; i < fileContentArray.Length; i += 2)
        {
            shootingGuides.Add(fileContentArray[i + 1]);
        }
    }
    #endregion

    public virtual void Pacify()
	{
		calmCurrent += 5;
		UpdateCalmness();
		if (calmCurrent >= calmMax) 
			GetParent<Node2D>().QueueFree();
	}

	private void UpdateCalmness()
	{
        calmMeter.Value = calmCurrent;
    }

	
}
