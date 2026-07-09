using System;

namespace DeepForest.Cards
{
    [Flags]
    public enum CardEffectTag : long
    {
        None            = 0,
        
        // === 傷勢效果 ===
        BlindnessLeft   = 1L << 0,   // 失明（左眼）
        BlindnessRight  = 1L << 1,   // 失明（右眼）
        Fracture        = 1L << 2,   // 骨折
        BrokenArm       = 1L << 3,   // 手臂脫臼
        BrokenFinger    = 1L << 4,   // 手指骨折
        TextDistortion  = 1L << 5,   // 文字扭曲
        Hallucination   = 1L << 6,   // 幻聽
        WoundInfection  = 1L << 7,   // 傷口感染
        
        // === 詛咒效果 ===
        Corruption      = 1L << 8,   // 穢祟附身
        CurseMark       = 1L << 9,   // 詛咒印記
        
        // === 物品分類 ===
        Food            = 1L << 10,  // 食物類
        Water           = 1L << 11,  // 飲水類
        Medicine        = 1L << 12,  // 藥品類
        Weapon          = 1L << 13,  // 武器類
        Armor           = 1L << 14,  // 防具類
        Tool            = 1L << 15,  // 工具類
        Light           = 1L << 16,  // 光源類
        
        // === 特殊標記 ===
        Diary           = 1L << 17,  // 日記/文獻
        QuestItem       = 1L << 18,  // 任務物品
        Loot            = 1L << 19,  // 戰利品
        UnequipCard     = 1L << 20,  // 卸下卡
        
        // === 戰鬥相關 ===
        StrBonus        = 1L << 21,  // 力量加成
        DexBonus        = 1L << 22,  // 敏捷加成
        WisBonus        = 1L << 23,  // 智慧加成
    }
}
