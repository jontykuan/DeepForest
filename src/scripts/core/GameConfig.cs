namespace DeepForest.Core;

public static class GameConfig
{
    // --- 角色基礎預設值 ---
    public const int DefaultMaxHp = 100;
    public const int DefaultMaxSanity = 100;
    public const int DefaultMaxHunger = 50;
    public const int DefaultMaxThirst = 50;

    // --- 牌組/手牌參數 ---
    public const int DefaultDraw = 5;
    public const int DefaultHandLimit = 7;
    public const int DefaultDeckCapacity = 30;

    // --- 每日飢餓與口渴極限懲罰 (L75-L87 in TurnManager) ---
    public const int StarvationHpPenalty = 5;
    public const int StarvationSanityPenalty = 5;
    public const int DehydrationHpPenalty = 5;
    public const int DehydrationSanityPenalty = 5;

    // --- 傷口感染惡化扣體力 ---
    public const int WoundInfectionHpPenalty = 3;

    // --- 穢祟附身扣理智 ---
    public const int CorruptionCurseSanityPenalty = 5;

    // --- 降神邪氣侵蝕扣理智 ---
    public const int DescentSanityPenalty = 5;

    // --- 每日換日結算預設值 (TurnManager L221-L229) ---
    public const int DefaultHungerDepletion = 3;
    public const int DefaultThirstDepletion = 3;
    public const int DefaultBaseHpRecovery = 8;
    public const int DefaultBaseSanityRecovery = 5;

    // --- 換日結算固定值與比例 ---
    public const int TurnEndEnvironmentHpPenalty = 5; // L237 in TurnManager

    // --- 結局判定閾值 ---
    public const int EndingBrutalityThreshold = 80;
    public const int EndingCorruptionThreshold = 80;
    public const int EndingEvilThreshold = 80;
    public const int EndingMaxDaysSurvival = 30;

    // --- 戰鬥參數 (CombatManager) ---
    public const int MinDamageFloor = 1;
    public const int DexDefenseDivisor = 2;
    public const int FleeSanityPenalty = 5;
}
