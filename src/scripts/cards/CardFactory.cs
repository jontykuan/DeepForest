using System;
using Godot;

namespace DeepForest.Cards
{
    public static class CardFactory
    {
        public static Card CreateCard(CardId id)
        {
            string path = id switch
            {
                // Action cards
                CardId.ActionStrength => "res://src/resources/cards/card_strength.tres",
                CardId.ActionStrength2 => "res://src/resources/cards/card_strength_2.tres",
                CardId.ActionStrength3 => "res://src/resources/cards/card_strength_3.tres",
                CardId.ActionDexterity => "res://src/resources/cards/card_dexterity.tres",
                CardId.ActionDexterity2 => "res://src/resources/cards/card_dexterity_2.tres",
                CardId.ActionDexterity3 => "res://src/resources/cards/card_dexterity_3.tres",
                CardId.ActionWisdom => "res://src/resources/cards/card_wisdom.tres",
                CardId.ActionWisdom2 => "res://src/resources/cards/card_wisdom_2.tres",
                CardId.ActionWisdom3 => "res://src/resources/cards/card_wisdom_3.tres",
                CardId.ActionRest => "res://src/resources/cards/card_rest.tres",
                CardId.ActionPlan => "res://src/resources/cards/card_plan.tres",
                CardId.ActionReorganize => "res://src/resources/cards/card_rearrange.tres",
                CardId.ActionBoast => "res://src/resources/cards/card_boast.tres",
                CardId.ActionSearchBackpack => "res://src/resources/cards/card_search_backpack.tres",
                CardId.ActionWhistle => "res://src/resources/cards/card_whistle.tres",
                CardId.ActionHideAndSeek => "res://src/resources/cards/card_hide_and_seek.tres",
                CardId.ActionPeekABoo => "res://src/resources/cards/card_peek_a_boo.tres",
                CardId.ActionSelfHarm => "res://src/resources/cards/card_self_harm.tres",

                // Consumables
                CardId.ConsumableAntidepressant => "res://src/resources/cards/card_antidepressant.tres",
                CardId.ConsumableBandage => "res://src/resources/cards/card_bandage.tres",
                CardId.ConsumableBullet => "res://src/resources/cards/card_bullet.tres",
                CardId.ConsumableChocolate => "res://src/resources/cards/card_chocolate.tres",
                CardId.ConsumableDefenseSpray => "res://src/resources/cards/card_defense_spray.tres",
                CardId.ConsumableDogFood => "res://src/resources/cards/card_dog_food.tres",
                CardId.ConsumableFirstAidKit => "res://src/resources/cards/card_first_aid_kit.tres",
                CardId.UnusedConsumableFlare => null,
                CardId.ConsumableHerb => "res://src/resources/cards/card_herb.tres",
                CardId.ConsumableRations3 => "res://src/resources/cards/card_rations.tres",
                CardId.ConsumableRations2 => "res://src/resources/cards/card_rations_2.tres",
                CardId.ConsumableRations1 => "res://src/resources/cards/card_rations_1.tres",
                CardId.ConsumableRawFish => "res://src/resources/cards/card_raw_fish.tres",
                CardId.ConsumableRawWater => "res://src/resources/cards/card_raw_water.tres",
                CardId.ConsumableRepellent => "res://src/resources/cards/card_repellent.tres",
                CardId.ConsumableRiceBall => "res://src/resources/cards/card_rice_ball.tres",
                CardId.ConsumableSleepingPill => "res://src/resources/cards/card_sleeping_pill.tres",
                CardId.ConsumableStimulant => "res://src/resources/cards/card_stimulant.tres",
                CardId.ConsumableWater => "res://src/resources/cards/card_water_flask.tres",
                CardId.ConsumableWildBerry => "res://src/resources/cards/card_wild_berry.tres",
 
                // Equipment
                CardId.EquipmentBackpack => "res://src/resources/cards/card_backpack.tres",
                CardId.EquipmentClimbingRope => "res://src/resources/cards/card_climbing_rope.tres",
                CardId.EquipmentCompass => "res://src/resources/cards/card_compass.tres",
                CardId.EquipmentFlashlight => "res://src/resources/cards/card_flashlight.tres",
                CardId.EquipmentHandcuffs => "res://src/resources/cards/card_handcuffs.tres",
                CardId.EquipmentLighter => "res://src/resources/cards/card_lighter.tres",
                CardId.EquipmentMultitool => "res://src/resources/cards/card_multitool.tres",
                CardId.EquipmentPistol => "res://src/resources/cards/card_pistol.tres",
                CardId.EquipmentRaincoat => "res://src/resources/cards/card_windbreaker.tres",
                CardId.EquipmentPoliceBaton => "res://src/resources/cards/card_police_baton.tres",
                CardId.EquipmentFlamethrower => "res://src/resources/cards/card_flamethrower.tres",

                // Passives
                CardId.PassiveCrayon => "res://src/resources/cards/card_crayon.tres",
                CardId.PassiveFountainPen => "res://src/resources/cards/card_fountain_pen.tres",

                // Injuries
                CardId.InjuryCut => "res://src/resources/cards/card_cut.tres",
                CardId.InjuryFatigue => "res://src/resources/cards/card_fatigue.tres",

                // Keys / Items
                CardId.KeyAllianceResolve => "res://src/resources/cards/item_alliance_resolve.tres",
                CardId.KeyShowdown => "res://src/resources/cards/item_alliance_resolve.tres",
                CardId.KeyBabySon => "res://src/resources/cards/item_baby_son.tres",
                CardId.UnusedKeyBloodContract => null,
                CardId.UnusedKeyBloodyHandkerchief => null,
                CardId.KeyBrokenPhone => "res://src/resources/cards/item_broken_phone.tres",
                CardId.KeyBrotherFavor => "res://src/resources/cards/item_brother_favor.tres",
                CardId.KeyButton => "res://src/resources/cards/item_button.tres",
                CardId.KeyCondomPackaging => "res://src/resources/cards/item_condom_packaging.tres",
                CardId.UnusedKeyDeathNotification => null,
                CardId.UnusedKeyDescentBrand => null,
                CardId.CurseMadness => "res://src/resources/cards/item_descent_madness.tres",
                CardId.UnusedKeyDiary => null,
                CardId.KeyDivorceAgreement => "res://src/resources/cards/item_divorce.tres",
                CardId.KeyJerryCollar => "res://src/resources/cards/item_dog_collar.tres",
                CardId.UnusedKeyEmbers => null,
                CardId.KeyEmptyPillBottle => "res://src/resources/cards/item_empty_pill_bottle.tres",
                CardId.KeyTinyFootprints => "res://src/resources/cards/item_footprint.tres",
                CardId.KeySneakPhoto => "res://src/resources/cards/item_infidelity_evidence.tres",
                CardId.KeyInsurancePolicy => "res://src/resources/cards/item_insurance_policy.tres",
                CardId.UnusedKeyLedger => null,
                CardId.KeyLockedDiary => "res://src/resources/cards/item_locked_diary.tres",
                CardId.KeyDiagnosisCert => "res://src/resources/cards/item_medical_diagnosis.tres",
                CardId.KeyOldKey => "res://src/resources/cards/item_old_key.tres",
                CardId.KeyOldScripture => "res://src/resources/cards/item_old_scripture.tres",
                CardId.KeyPainfulTruth => "res://src/resources/cards/item_painful_truth.tres",
                CardId.KeyPromissoryNote => "res://src/resources/cards/item_promissory_note.tres",
                CardId.KeyRecordingTape => "res://src/resources/cards/item_recording.tres",
                CardId.UnusedKeyRemnantHumanity => null,
                CardId.KeyScheme => "res://src/resources/cards/item_scheme.tres",
                CardId.KeyScrapMetal => "res://src/resources/cards/item_scrap_metal.tres",
                CardId.KeySeedOfLife => "res://src/resources/cards/item_seed_of_life.tres",
                CardId.KeyRoster => "res://src/resources/cards/item_sleazy_flier.tres",
                CardId.KeyTornFairytale => "res://src/resources/cards/item_torn_fairytale.tres",
                CardId.KeyChastityLie => "res://src/resources/cards/item_unblemished_past.tres",
                CardId.KeyWhisper => "res://src/resources/cards/item_whisper.tres",
                CardId.KeyWood => "res://src/resources/cards/item_wood.tres",
                CardId.KeyJerry => "res://src/resources/cards/card_jerry.tres",
                CardId.KeyJerryQuestion => "res://src/resources/cards/card_jerry_fake.tres",
                CardId.KeyMapFragment => "res://src/resources/cards/card_map.tres",

                _ => null
            };

            if (path != null && Godot.ResourceLoader.Exists(path))
            {
                var card = Godot.GD.Load<Card>(path);
                if (card != null)
                {
                    card.CardId = id;
                    if (id == CardId.KeyShowdown || id == CardId.KeyAllianceResolve)
                    {
                        card.ThirstCost = 2;
                    }
                    AssignEffectTags(card);
                    return card;
                }
            }

            return CreateCardFallback(id);
        }

        public static Card CreateActionCard(CardId id, string name, CardClass @class, int weight, int str, int dex, int wis, int hungerCost, int thirstCost)
        {
            var card = new Card
            {
                CardId = id,
                CardName = name,
                CardClass = @class,
                Weight = weight,
                StrValue = str,
                DexValue = dex,
                WisValue = wis,
                HungerCost = hungerCost,
                ThirstCost = thirstCost
            };
            AssignEffectTags(card);
            return card;
        }

        public static Card CreateConsumableCard(CardId id, string name, int hp, int sanity, int hunger, int thirst)
        {
            var card = new Card
            {
                CardId = id,
                CardName = name,
                CardClass = CardClass.Consumable,
                Weight = 1,
                HpCost = hp,
                SanityCost = sanity,
                HungerCost = hunger,
                ThirstCost = thirst
            };
            AssignEffectTags(card);
            return card;
        }

        public static Card CreateEquipmentCard(CardId id, string name, string desc, int weight = 1)
        {
            var card = new Card
            {
                CardId = id,
                CardName = name,
                CardClass = CardClass.Equipment,
                Weight = weight,
                Description = desc
            };
            AssignEffectTags(card);
            return card;
        }

        public static Card CreateKeyItemCard(CardId id, string name, string desc, int weight = 1)
        {
            var card = new Card
            {
                CardId = id,
                CardName = name,
                CardClass = CardClass.KeyItem,
                Weight = weight,
                Description = desc
            };
            AssignEffectTags(card);
            return card;
        }

        public static Card CreateInjuryCard(CardId id, string name, string desc)
        {
            var card = new Card
            {
                CardId = id,
                CardName = name,
                CardClass = CardClass.Injury,
                Weight = 1,
                Description = desc
            };
            AssignEffectTags(card);
            return card;
        }

        public static Card CreateCurseCard(CardId id, string name, string desc)
        {
            var card = new Card
            {
                CardId = id,
                CardName = name,
                CardClass = CardClass.Curse,
                Weight = 1,
                Description = desc
            };
            AssignEffectTags(card);
            return card;
        }

        public static void AssignEffectTags(Card card)
        {
            // Reset
            card.EffectTags = CardEffectTag.None;

            // Map CardId to Effect Tags
            switch (card.CardId)
            {
                case CardId.InjuryBlindnessLeft:
                    card.EffectTags |= CardEffectTag.BlindnessLeft;
                    break;
                case CardId.InjuryBlindnessRight:
                    card.EffectTags |= CardEffectTag.BlindnessRight;
                    break;
                case CardId.InjuryFracture:
                    card.EffectTags |= CardEffectTag.Fracture;
                    break;
                case CardId.InjuryDislocation:
                    card.EffectTags |= CardEffectTag.BrokenArm;
                    break;
                case CardId.InjuryFingerFracture:
                    card.EffectTags |= CardEffectTag.BrokenFinger;
                    break;
                case CardId.InjuryTextDistortion:
                    card.EffectTags |= CardEffectTag.TextDistortion;
                    break;
                case CardId.InjuryHallucination:
                    card.EffectTags |= CardEffectTag.Hallucination;
                    break;
                case CardId.InjuryInfection:
                    card.EffectTags |= CardEffectTag.WoundInfection;
                    break;
                case CardId.InjuryPossession:
                    card.EffectTags |= CardEffectTag.Corruption;
                    break;
                case CardId.UnusedKeyDiary:
                    card.EffectTags |= CardEffectTag.Diary;
                    break;
                case CardId.ConsumableWater:
                    card.EffectTags |= CardEffectTag.Water;
                    break;
                case CardId.ConsumableRawFish:
                case CardId.ConsumableFish:
                case CardId.ConsumableDriedFood:
                case CardId.ConsumableRations3:
                case CardId.ConsumableRations2:
                case CardId.ConsumableRations1:
                case CardId.ConsumableWolfMeat:
                    card.EffectTags |= CardEffectTag.Food;
                    break;
                case CardId.EquipmentKnife:
                    card.EffectTags |= CardEffectTag.Weapon;
                    break;
                case CardId.EquipmentWolfSkin:
                    card.EffectTags |= CardEffectTag.Armor;
                    break;
                case CardId.EquipmentWaterFlask:
                case CardId.EmptyWaterFlask:
                    card.EffectTags |= CardEffectTag.Tool;
                    break;
                case CardId.EquipmentTorch:
                    card.EffectTags |= CardEffectTag.Light;
                    break;
        }
    }

        private static Card CreateCardFallback(CardId id)
        {
            switch (id)
            {
                case CardId.KeyEmptyPillBottle:
                    return CreateKeyItemCard(id, "空藥瓶", "空藥瓶。");
                case CardId.CurseAddiction:
                    return CreateCurseCard(id, "成癮", "成癮。");
                case CardId.InjuryCut:
                    return CreateInjuryCard(id, "割痕", "割痕。");
                case CardId.InjuryFatigue:
                    return CreateInjuryCard(id, "疲勞", "疲勞。");
                case CardId.KeyScheme:
                    return CreateKeyItemCard(id, "心機", "心機。");
                case CardId.KeyDivorceAgreement:
                    return CreateKeyItemCard(id, "離婚協議書", "離婚協議書。");
                case CardId.KeyRecordingTape:
                    return CreateKeyItemCard(id, "錄音帶", "錄音帶。");
                case CardId.KeyPromissoryNote:
                    return CreateKeyItemCard(id, "本票", "本票。");
                case CardId.KeyChastityLie:
                    return CreateKeyItemCard(id, "貞潔謊言", "貞潔謊言。");
                case CardId.ActionRebellion:
                    return CreateActionCard(id, "反抗", CardClass.ActionStr, 1, 0, 0, 0, 0, 5); // Rebellion card fallback
                case CardId.ActionCopy:
                    return CreateActionCard(id, "複製", CardClass.ActionDex, 1, 0, 0, 0, 0, 0); // Copy card fallback
                case CardId.CurseSuffocation:
                    return CreateCurseCard(id, "窒息", "窒息。當卡片加入手牌時，丟棄所有手牌，並抽1張牌。");
                case CardId.KeyBabySon:
                    return CreateKeyItemCard(id, "寶貝兒子", "寶貝兒子。");
                case CardId.KeyShowdown:
                case CardId.KeyAllianceResolve:
                    {
                        var showdown = CreateKeyItemCard(id, "攤牌", "將隱藏多年的扭曲證據公諸於世的關鍵決意。");
                        showdown.ThirstCost = 2;
                        return showdown;
                    }
                case CardId.KeyRoster:
                case CardId.KeySleazyFlier:
                    return CreateKeyItemCard(id, "花名冊", "花名冊。");
                case CardId.KeyOldScripture:
                    return CreateKeyItemCard(id, "舊日教本", "舊日教本。");
                case CardId.KeyOldKey:
                    return CreateKeyItemCard(id, "老舊鑰匙", "老舊鑰匙。");
                case CardId.KeyPliers:
                    return CreateKeyItemCard(id, "鐵鉗", "用來破壞鎖鏈或解開不可解除裝備的工具。");
                case CardId.KeyBrokenPhone:
                    return CreateKeyItemCard(id, "壞掉的手機", "壞掉的手機。");
                case CardId.KeyLockedDiary:
                    return CreateKeyItemCard(id, "上鎖的日記", "上鎖的日記。");
                case CardId.KeySeedOfLife:
                    return CreateKeyItemCard(id, "生命的種子", "生命的種子。");
                case CardId.KeyWhisper:
                    return CreateKeyItemCard(id, "耳語", "耳語。");
                case CardId.KeyDiagnosisCert:
                    return CreateKeyItemCard(id, "診斷證明書", "診斷證明書。");
                case CardId.KeyJerry:
                    return CreateKeyItemCard(id, "傑利", "傑利。");
                case CardId.KeyJerryQuestion:
                    return CreateKeyItemCard(id, "傑利？", "傑利？。");
                case CardId.KeyJerryCollar:
                    return CreateEquipmentCard(id, "傑利的項圈", "真心希望你不要這麼做，裝備後理智上限-10。", 1);
                case CardId.KeyTinyFootprints:
                    return CreateKeyItemCard(id, "小小的足跡", "雪地中留下的小小足跡。");
                case CardId.ConsumableChocolate:
                    return CreateConsumableCard(id, "巧克力", 0, -10, -10, 0);
                case CardId.ConsumableAntidepressant:
                    return CreateConsumableCard(id, "抗憂鬱藥物", 0, -25, 0, 0);
                case CardId.ConsumableSleepingPill:
                    return CreateConsumableCard(id, "安眠藥", 0, -40, 0, 0);
                case CardId.ConsumableAlcohol:
                    return CreateConsumableCard(id, "烈酒", 0, -10, 0, 3);
                case CardId.ConsumableStimulant:
                    return CreateConsumableCard(id, "興奮劑", 0, 15, 0, 0);
                case CardId.ConsumableDefenseSpray:
                    return CreateConsumableCard(id, "防身噴霧", 0, 0, 0, 0);
                default:
                    string enumName = id.ToString();
                    if (enumName.StartsWith("Action"))
                    {
                        return CreateActionCard(id, enumName, CardClass.ActionStr, 1, 0, 0, 0, 0, 0);
                    }
                    else if (enumName.StartsWith("Consumable"))
                    {
                        return CreateConsumableCard(id, enumName, 0, 0, 0, 0);
                    }
                    else if (enumName.StartsWith("Equipment"))
                    {
                        return CreateEquipmentCard(id, enumName, enumName, 1);
                    }
                    else if (enumName.StartsWith("Injury"))
                    {
                        return CreateInjuryCard(id, enumName, enumName);
                    }
                    else if (enumName.StartsWith("Curse"))
                    {
                        return CreateCurseCard(id, enumName, enumName);
                    }
                    else
                    {
                        return CreateKeyItemCard(id, enumName, enumName);
                    }
            }
        }
    }
}
