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
    [Export] public CardId CardId { get; set; } = CardId.None;
    [Export] public string CardName { get; set; } = "New Card";
    [Export] public CardClass CardClass { get; set; } = CardClass.ActionStr;

    // Backwards compatibility alias
    public CardType CardType
    {
        get
        {
            return CardClass switch
            {
                CardClass.ActionStr => CardType.ActionStr,
                CardClass.ActionDex => CardType.ActionDex,
                CardClass.ActionWis => CardType.ActionWis,
                CardClass.Consumable => CardType.Consumable,
                CardClass.Equipment => CardType.Equipment,
                CardClass.KeyItem => CardType.KeyItem,
                CardClass.Curse => CardType.Curse,
                CardClass.Injury => CardType.Injury,
                CardClass.Passive => CardType.Passive,
                _ => CardType.ActionStr
            };
        }
        set
        {
            CardClass = value switch
            {
                CardType.ActionStr => CardClass.ActionStr,
                CardType.ActionDex => CardClass.ActionDex,
                CardType.ActionWis => CardClass.ActionWis,
                CardType.Consumable => CardClass.Consumable,
                CardType.Equipment => CardClass.Equipment,
                CardType.KeyItem => CardClass.KeyItem,
                CardType.Curse => CardClass.Curse,
                CardType.Injury => CardClass.Injury,
                CardType.Passive => CardClass.Passive,
                _ => CardClass.ActionStr
            };
        }
    }

    [Export] public CardEffectTag EffectTags { get; set; } = CardEffectTag.None;
    [Export] public int Weight { get; set; } = 1;
    [Export] public int StrValue { get; set; } = 0;
    [Export] public int DexValue { get; set; } = 0;
    [Export] public int WisValue { get; set; } = 0;
    [Export] public int HungerCost { get; set; } = 0;
    [Export] public int ThirstCost { get; set; } = 0;
    [Export] public int HpCost { get; set; } = 0;
    [Export] public int SanityCost { get; set; } = 0;
    [Export] public int BrutalityChange { get; set; } = 0;
    [Export] public int CorruptionChange { get; set; } = 0;
    [Export] public int EvilChange { get; set; } = 0;
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";

    [Export] public int MaxUses { get; set; } = 0;
    public int UsesLeft { get; set; }

    public string DisplayName => (MaxUses > 0) ? $"{CardName}({UsesLeft}/{MaxUses})" : CardName;
    public string DisplayDescription => (MaxUses > 0) ? $"{Description}\n[剩餘使用次數: {UsesLeft}/{MaxUses}]" : Description;

    [Export] public Narrative.EventEffect? PlayEffect { get; set; }
}
