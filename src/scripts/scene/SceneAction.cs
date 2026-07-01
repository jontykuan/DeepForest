using Godot;

namespace DeepForest.Scene;

public enum ActionEffectType
{
    None,
    Camp,
    Fish,
    CollectWater,
    Search,
    PryCellar,
    MoveForward,
    ExploreIndoor,
    LeaveIndoor,
    ReturnOutdoor,
    LootCorpse,
    DissectCorpse,
    CombatClash,
    OpenWoodChest,
    OpenIronChest,
    TouchCursedChest,
    EnterNormalCabin,
    EnterStrangeCabin,
    EnterCave,
    TradeHunter,
    WitchRitual
}

public enum ThresholdType
{
    None,
    Str,
    Dex,
    Wis,
    Any
}

[GlobalClass]
public partial class SceneAction : Resource
{
    [Export] public string ActionName { get; set; } = "Action";
    [Export] public ThresholdType ThresholdType { get; set; } = ThresholdType.None;
    [Export] public int ThresholdValue { get; set; } = 0;
    [Export] public string RequiredItem { get; set; } = "";
    [Export] public ActionEffectType EffectType { get; set; } = ActionEffectType.None;
    [Export] public int HpCostOnComplete { get; set; } = 2; 
}
