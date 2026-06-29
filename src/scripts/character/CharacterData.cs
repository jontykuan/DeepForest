using Godot;
using DeepForest.Cards;

namespace DeepForest.Character;

[GlobalClass]
public partial class CharacterData : Resource
{
    [Export] public string CharacterName { get; set; } = "New Character";
    [Export] public int MaxHp { get; set; } = 100;
    [Export] public int MaxSanity { get; set; } = 100;
    [Export] public int MaxHunger { get; set; } = 50;
    [Export] public int MaxThirst { get; set; } = 50;
    [Export] public int Draw { get; set; } = 5;
    [Export] public int HandLimit { get; set; } = 7;
    [Export] public int DeckCapacity { get; set; } = 30;
    [Export] public Godot.Collections.Array<Card> StartingDeck { get; set; } = new();
}
