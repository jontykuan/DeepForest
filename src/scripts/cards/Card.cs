using Godot;

namespace DeepForest.Cards;

public enum CardType
{
    ActionStr,
    ActionDex,
    ActionWis,
    Consumable,
    Equipment,
    Passive,
    KeyItem,
    Curse,
    Injury
}

[GlobalClass]
public partial class Card : Resource
{
    [Export] public string CardName { get; set; } = "New Card";
    [Export] public CardType CardType { get; set; } = CardType.ActionStr;
    [Export] public int Weight { get; set; } = 1;
    [Export] public int StrValue { get; set; } = 0;
    [Export] public int DexValue { get; set; } = 0;
    [Export] public int WisValue { get; set; } = 0;
    [Export] public int HungerCost { get; set; } = 0;
    [Export] public int ThirstCost { get; set; } = 0;
    [Export] public int HpCost { get; set; } = 0;
    [Export] public int SanityCost { get; set; } = 0;
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
}
