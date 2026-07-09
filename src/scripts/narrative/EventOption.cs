using Godot;
using DeepForest.Scene;

namespace DeepForest.Narrative
{
    [GlobalClass]
    public partial class EventOption : Resource
    {
        [Export] public string OptionText { get; set; } = "選擇";
        [Export] public ThresholdType ThresholdType { get; set; } = ThresholdType.None;
        [Export] public int ThresholdValue { get; set; } = 0;
        [Export] public Cards.CardId RequiredCardId { get; set; } = Cards.CardId.None;
        [Export] public ActionEffectType EffectType { get; set; } = ActionEffectType.None;

        // Outcomes
        [Export] public string LogMessageOnSuccess { get; set; } = "";
        [Export] public string LogMessageOnFailure { get; set; } = "";

        // Stat adjustments
        [Export] public int HpChange { get; set; } = 0;
        [Export] public int SanityChange { get; set; } = 0;
        [Export] public int HungerChange { get; set; } = 0;
        [Export] public int ThirstChange { get; set; } = 0;
        [Export] public int BrutalityChange { get; set; } = 0;
        [Export] public int CorruptionChange { get; set; } = 0;
        [Export] public int EvilChange { get; set; } = 0;

        // Card additions/removals
        [Export] public Cards.CardId CardIdToGive { get; set; } = Cards.CardId.None;
        [Export] public Cards.CardId CardIdToTake { get; set; } = Cards.CardId.None;

        // Custom scene transitions
        [Export] public int NextNodeIdOverride { get; set; } = -1;
        [Export] public string NextIndoorSceneOverride { get; set; } = "";
    }
}
