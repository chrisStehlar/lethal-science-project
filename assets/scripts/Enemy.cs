using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class Enemy : StaticBody2D
{
	// Misc. Aesthetic variables
	[Export] string enemyName;
	[Export] Node conductor;
	[Export] int baseCalmMax;
	AnimatedSprite2D sprite;

	// Audio variables
	[Export] AudioStreamWav pacifySound;
	AudioStreamPlayer soundPlayer;

	// Calm meter settings
	int calmMax;
	int calmCurrent;
	[Export] TextureProgressBar calmMeter;

	// Projectile-related variables
	[Export] PackedScene projectileScene;
	[Export] Color projectileColor;
	[Export] int projectileSpeed;
	[Export] float projectileRange;				//In radians
	[Export] Difficulty difficulty;

	// Shooting Guide variables
	string initialShootingGuide;
	string loopShootingGuide;
	string currentShootingGuide;			//Set to initial or loop, and read by the function
	int currentGuideIndex;
    int guideLength;

	public int CalmCurrent
	{
		get { return calmCurrent; }
	}
	public int CalmMax
	{
		get { return calmMax; }
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		currentGuideIndex = 0;
		if (calmMeter != null)
		{
            calmMax = baseCalmMax + baseCalmMax / 2 * (int)GameManager.Instance.Difficulty;
            calmCurrent = 0;
			calmMeter.MaxValue = calmMax;
			calmMeter.TintOver = projectileColor;
        }
		LoadShootingGuide();
        currentShootingGuide = initialShootingGuide;
        guideLength = currentShootingGuide.Length;

        sprite = GetChild<AnimatedSprite2D>(0);
		sprite.AnimationFinished += () => { sprite.Animation = "idle"; };
		
		soundPlayer = new AudioStreamPlayer();
		soundPlayer.Stream = pacifySound;
		soundPlayer.VolumeDb -= 8;
		AddChild(soundPlayer);
	}

	/// <summary>
	/// Instead of _Process() let's use Beat() to run logic every beat.
	/// </summary>
	public void Beat(int beatIndex)
	{
		if (initialShootingGuide == "-1" && loopShootingGuide == "-1") return;
		if (GameManager.Instance.Difficulty < difficulty) return;

		if (currentGuideIndex >= guideLength) ResetGuide(beatIndex);
		// Contingencies
		if (!(bool)conductor.Get("IsPlaying")) return;
		if (string.IsNullOrEmpty(currentShootingGuide))
		{
			SpawnProjectile();
			return;
		}
		if ((bool)conductor.Get("PrintToConsoleEnabled")) {
			GD.Print("Current Note: " + beatIndex);
		}

		//Convert the guide number to an int
		int guideNumber = currentShootingGuide[currentGuideIndex] - '0';
		if (guideNumber == 1)
			SpawnProjectile();
		else if (guideNumber > 0)
		{
			for (int i = 0; i < guideNumber; i++)
				SpawnProjectile();
		}
        currentGuideIndex++;
    }

    #region Shooting Guide Functions
    private void SpawnProjectile()
	{
		Projectile projectile = projectileScene.Instantiate() as Projectile;
		projectile.GlowColor = projectileColor;
		projectile.Speed = projectileSpeed + (int)GameManager.Instance.Difficulty * 2 - (int)difficulty * 2;
		projectile.Orientation = GD.Randf() * (projectileRange * 2) - projectileRange;
		projectile.Position += new Vector2(0, 10);
		AddChild(projectile);
		sprite.Animation = "swing";
		sprite.Play();
	}

	public void ResetGuide(int beatIndex)
	{
		if (currentShootingGuide != loopShootingGuide) currentShootingGuide = loopShootingGuide;
		currentGuideIndex = 0;
        guideLength = currentShootingGuide.Length;
	}
    private void LoadShootingGuide()
    {
        string filePath = "res://guides/" + (string)conductor.Get("TrackName") + "/" + enemyName + ".csv";
		GD.Print(filePath);

		try
		{
            using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
            string fileContent = file.GetAsText();
            string[] fileContentArray = fileContent.Split(',');

            //File content is read in groups of 3:
            //1 - whether initial or loop, represented as i
            //2 - repetition count, represented as i + 1
            //3 - guide numbers, represented as i + 2
            for (int i = 0; i < fileContentArray.Length; i += 3)
            {
                int repetitionCount = int.Parse(fileContentArray[i + 1]);
				
                for (int j = 0; j < repetitionCount; j++)
                {
                    if (fileContentArray[i] == "initial")
                        initialShootingGuide += fileContentArray[i + 2];
                    else
                        loopShootingGuide += fileContentArray[i + 2];

                }
            }
            
        }
		catch (Exception e){
			GD.Print(e.Message);
			initialShootingGuide = "-1";
			loopShootingGuide = "-1";
			return; 
		}
    }
    #endregion

    public virtual void Pacify()
	{
		soundPlayer.Play();
		sprite.Play("damage");
		calmCurrent += 5;
		UpdateCalmness();
		if (calmCurrent >= calmMax) End();
	}

	private void UpdateCalmness()
	{
		Tween calmMeterTween = CreateTween();
        calmMeterTween.TweenProperty(calmMeter, "value", calmCurrent, 0.5).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		
    }	

	private void End()
	{

		
        //GetParent<Node2D>().QueueFree();
    }
}
