using Godot;

namespace DeepForest.Combat;

[GlobalClass]
public partial class Enemy : Resource
{
    [Export] public string EnemyName { get; set; } = "Forest Monster";
    [Export] public int MaxHp { get; set; } = 30;
    [Export] public int CurrentHp { get; set; } = 30;
    [Export] public int BaseDamage { get; set; } = 8;
    [Export] public int BaseSanityDamage { get; set; } = 5;
    [Export] public string Description { get; set; } = "A terrifying presence in the shadows.";
}
