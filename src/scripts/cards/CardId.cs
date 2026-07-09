namespace DeepForest.Cards;

public enum CardId
{
    None = 0,

    // ─── 行動牌 (Action) ───
    ActionSlash,
    ActionKick,
    ActionForce,
    ActionClimb,
    ActionDash,
    ActionSprint,
    ActionDig,
    ActionSneak,
    ActionLockpick,
    ActionScout,
    ActionTrack,
    ActionSearch,
    ActionLure,
    ActionTrap,
    ActionFishing,
    ActionGather,
    ActionScavenge,
    ActionNegotiate,
    ActionPrayer,
    ActionTeach,
    ActionFirstAid,
    ActionReorganize,
    ActionImprovise,
    ActionSignal,
    ActionSteady,
    ActionThrow,
    ActionBite,
    ActionBark,
    ActionBodySlam,
    ActionSelfHarm,
    ActionStrength,
    ActionStrength2,
    ActionStrength3,
    ActionDexterity,
    ActionDexterity2,
    ActionDexterity3,
    ActionWisdom,
    ActionWisdom2,
    ActionWisdom3,
    ActionRest,
    ActionSearchBackpack,
    ActionBoast,
    ActionRebellion,        // 反抗
    ActionCopy,             // 複製
    ActionHideAndSeek,
    ActionPeekABoo,
    ActionPlan,
    ActionWhistle,

    // ─── 消耗品 (Consumable) ───
    ConsumableDriedFood,
    ConsumableEnergyBar,
    ConsumableWater, // 清水 / 甘甜泉水
    ConsumableBandage,
    ConsumableAlcohol,
    ConsumableAntidepressant,
    ConsumableSleepingPill,
    ConsumableStimulant,
    ConsumableDefenseSpray,
    ConsumableRepellent,
    ConsumableRawFish,
    ConsumableRawWater, // 生水
    ConsumableRiceBall,
    ConsumableWildBerry,
    ConsumableRations3, // 營養口糧(3/3)
    ConsumableRations2, // 營養口糧(2/3)
    ConsumableRations1, // 營養口糧(1/3)
    ConsumableChocolate, // 巧克力糖
    ConsumableDogFood, // 狗食
    ConsumableBullet, // 子彈
    UnusedConsumableFlare, // 信號彈
    ConsumableHerb, // 草藥
    ConsumableWolfMeat, // 狼肉 / 烤狼肉
    ConsumableFish, // 烤魚
    ConsumableFirstAidKit, // 急救箱

    // ─── 裝備 (Equipment) ───
    EquipmentKnife, // 柴刀 / 刀
    EquipmentFlashlight, // 手電筒
    EquipmentClimbingRope, // 登山繩
    EquipmentRaincoat, // 防風外套 / 雨衣
    EquipmentSturdyBoots, // 登山鞋
    EquipmentHandcuffs, // 手銬
    EquipmentLighter, // 打火機
    EquipmentBackpack, // 登山背包
    EquipmentTorch, // 火把
    EquipmentWaterFlask, // 水瓶
    EquipmentWolfSkin, // 狼皮
    EquipmentCompass, // 指北針
    EquipmentMultitool, // 多功能刀
    EquipmentPistol, // 手槍

    // ─── 被動 (Passive) ───
    PassiveOldPhoto,
    PassiveCrayon, // 蠟筆
    PassiveFountainPen, // 精緻鋼筆

    // ─── 關鍵物品 (KeyItem) ───
    KeyMapFragment,         // 地圖殘片
    KeyDivorceAgreement,    // 離婚協議書
    KeyRecordingTape,       // 錄音帶
    KeyPromissoryNote,      // 本票
    KeyChastityLie,         // 貞潔謊言
    KeyBabySon,             // 寶貝兒子
    KeyTinyFootprints,      // 小小的足跡
    KeyEmptyPillBottle,     // 空藥瓶 (Item)
    KeyDiagnosisCert,       // 診斷證明書
    KeySneakPhoto,          // 偷拍照片
    KeyRoster,              // 花名冊
    KeyShowdown,            // 攤牌
    KeyPainfulTruth,        // 痛苦真相
    KeyBrokenPhone,         // 壞掉的手機
    KeyLockedDiary,         // 上鎖的日記
    KeySeedOfLife,          // 生命的種子
    KeyWhisper,             // 耳語
    KeyOldScripture,        // 舊日教本
    KeyOldKey,              // 老舊鑰匙
    KeyScheme,              // 心機
    KeyJerryCollar,         // 傑利的項圈
    KeyJerry,               // 傑利
    KeyJerryQuestion,       // 傑利？
    KeyAllianceResolve,     // 同盟的決意
    UnusedKeyBloodContract,       // 降神契印 / 血契之證
    UnusedKeyBloodyHandkerchief,  // 帶血的手帕
    KeyBrotherFavor,        // 專寵
    KeyButton,              // 金屬鈕扣
    KeyCondomPackaging,     // 避孕套包裝
    UnusedKeyDeathNotification,   // 死亡通知書
    UnusedKeyDescentBrand,        // 降神契印
    UnusedKeyDiary,               // 秘密日記 / 帶血的日記
    UnusedKeyEmbers,              // 餘燼
    KeyInsurancePolicy,     // 高額保險單
    UnusedKeyRemnantHumanity,     // 殘存的人性
    KeyScrapMetal,          // 廢鐵
    KeySleazyFlier,         // 腥羶傳單
    KeyTornFairytale,       // 撕毀的童話書
    KeyWood,                // 木材
    UnusedKeyLedger,              // 債務帳單
    KeyPliers,              // 鐵鉗

    // ─── 負面/副產物 (Curse/Injury/Empty) ───
    CurseMadness,           // 狂亂
    CurseSuffocation,       // 窒息
    CurseOldShadow,         // 舊日殘影
    CurseAddiction,         // 成癮
    InjuryBlindnessLeft,    // 失明（左眼）
    InjuryBlindnessRight,   // 失明（右眼）
    InjuryFracture,         // 骨折
    InjuryDislocation,      // 手臂脫臼
    InjuryFingerFracture,   // 手指骨折
    InjuryTextDistortion,   // 文字扭曲
    InjuryHallucination,    // 幻聽
    InjuryInfection,        // 傷口感染
    InjuryPossession,       // 穢祟附身
    InjuryGastroenteritis,  // 腸胃炎
    InjuryCut,              // 割痕
    InjuryFatigue,          // 疲勞
    EmptyBottle,            // 空酒瓶 / 空瓶
    EmptyPillBottle,        // 空藥瓶 (副產物)
    EmptyWaterFlask,        // 空水瓶
    EquipmentPoliceBaton,   // 警棍
    EquipmentFlamethrower,  // 噴火器
}
