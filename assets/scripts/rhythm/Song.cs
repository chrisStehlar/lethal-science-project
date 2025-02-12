using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Song : Resource
{
    [Export] public Phrase[] Phrases {get; set; }

    public int Length => Phrases.Length; // in phrases
}