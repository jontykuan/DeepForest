using Godot;

namespace DeepForest.Core
{
    [GlobalClass]
    public partial class BalanceData : Resource
    {
        // Turn & Day recovery/costs
        [Export] public int BaseHpRecovery { get; set; } = 8;
        [Export] public int BaseSanityRecovery { get; set; } = 5;
        
        // Hunger & Thirst parameters
        [Export] public int ThirstDepletion { get; set; } = 15;
        [Export] public int HungerDepletion { get; set; } = 10;
        
        // Combat parameters
        [Export] public int FleeHpCost { get; set; } = 5;
        [Export] public int FleeSanityCost { get; set; } = 10;
        
        // Ending thresholds
        [Export] public int EndingBrutalityThreshold { get; set; } = 80;
        [Export] public int EndingCorruptionThreshold { get; set; } = 80;
        [Export] public int EndingEvilThreshold { get; set; } = 80;
    }
}
