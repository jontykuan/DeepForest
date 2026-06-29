using Godot;
using DeepForest.Cards;

namespace DeepForest.Combat;

[GlobalClass]
public partial class EnemyData : Resource
{
    [Export] public string EnemyName { get; set; } = "野狼";
    [Export] public int MaxHp { get; set; } = 3;
    [Export] public int AttackPower { get; set; } = 10;
    [Export] public bool IsAggressive { get; set; } = true;
    [Export] public bool HideHp { get; set; } = false;
    [Export] public string DecalName { get; set; } = "npc_right";

    [Export] public Godot.Collections.Array<Card> ActionDeck { get; set; } = new();
    [Export] public Godot.Collections.Array<Card> LootTable { get; set; } = new();
}
