using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DeepForest.Core;
using DeepForest.Cards;
using DeepForest.Cards.Effects;
using DeepForest.Character;
using DeepForest.Combat;

namespace DeepForest.UI
{
    public static class CardPlayHandler
    {
        public enum PlayResult
        {
            Success,
            InvalidCard,
            InsufficientPoints,
            CombatZoneAdded,
            CursePurged,
            Unequipped
        }

        private static readonly Dictionary<string, ICardPlayEffect> _effects = new();

        static CardPlayHandler()
        {
            try
            {
                var effectTypes = typeof(ICardPlayEffect).Assembly
                    .GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract 
                        && typeof(ICardPlayEffect).IsAssignableFrom(t));
                        
                foreach (var type in effectTypes)
                {
                    var attrs = type.GetCustomAttributes<CardPlayEffectAttribute>();
                    if (attrs != null && attrs.Any())
                    {
                        var instance = (ICardPlayEffect)Activator.CreateInstance(type)!;
                        foreach (var attr in attrs)
                        {
                            _effects[attr.CardName] = instance;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Godot.GD.PrintErr($"[CardPlayHandler] 載入卡牌 Strategy 失敗: {ex.Message}");
            }
        }

        public static PlayResult TryPlayCard(Card card, Player player, Deck deck, out string message)
        {
            // 1. 傷勢卡檢驗 (規則層面阻擋)
            if (card.CardType == CardType.Injury)
            {
                message = "【傷勢】這是傷勢卡，你無法主動打出它！";
                return PlayResult.InvalidCard;
            }

            if (card.CardType == CardType.KeyItem)
            {
                message = $"【物品】這是重要劇情物品【{card.CardName}】，無法直接打出。";
                return PlayResult.InvalidCard;
            }

            // 2. 穢祟詛咒驅除檢驗
            if (card.CardId == CardId.InjuryPossession || card.EffectTags.HasFlag(CardEffectTag.Corruption))
            {
                if (player.CurrentSanity < 15)
                {
                    message = "【詛咒】你的理智不足 15，無法驅除此詛咒！";
                    return PlayResult.InsufficientPoints;
                }
                player.CurrentSanity -= 15;
                deck.Hand.Remove(card); 
                message = $"你忍受著劇烈精神痛苦，消耗了 15 點理智，永久驅散了【{card.CardName}】！";
                return PlayResult.CursePurged;
            }

            // 3. 戰鬥模式路由
            if (GameState.Instance.IsInCombat)
            {
                if (card.CardType == CardType.ActionStr || card.CardType == CardType.ActionDex || card.CardType == CardType.ActionWis)
                {
                    CombatManager.Instance.AddCardToCombatZone(card);
                    message = $"打出屬性卡【{card.CardName}】放入對決區。";
                    return PlayResult.CombatZoneAdded;
                }
            }

            // 4. 計算環境與背包重量對消耗的加成
            int hungerCost = card.HungerCost;
            int thirstCost = card.ThirstCost;
            if (card.CardType != CardType.Consumable && thirstCost > 0 && EnvironmentSystem.Instance != null)
            {
                thirstCost += EnvironmentSystem.Instance.GetThirstCostModifier();
            }

            int overweight = deck.GetTotalWeight() - player.DeckCapacity;
            if (overweight > 0)
            {
                int penalty = 0;
                if (overweight <= 5) penalty = 1;
                else if (overweight <= 10) penalty = 2;
                else if (overweight <= 20) penalty = 4;
                else penalty = 6;

                hungerCost += penalty;
                thirstCost += penalty;
                GameState.Instance.AddLog($"【超重負載】背包超重 {overweight}，出牌額外消耗了 {penalty} 飢餓與口渴！");
            }

            // 5. 「疲勞」手牌被動懲罰扣點
            if (card.CardId != CardId.InjuryFatigue)
            {
                int fatigueCount = deck.Hand.Count(c => c.CardId == CardId.InjuryFatigue);
                if (fatigueCount > 0)
                {
                    player.CurrentHunger -= fatigueCount;
                    player.CurrentThirst -= fatigueCount;
                    GameState.Instance.AddLog($"【疲勞被動】手牌中的 {fatigueCount} 張【疲勞】使你額外消耗了 {fatigueCount} 飢餓與 {fatigueCount} 口渴。");
                }
            }

            // 6. 裝備卸載判定 (使用 Meta)
            if (card.HasMeta("parent_card"))
            {
                Card parentCard = (Card)card.GetMeta("parent_card");
                if (player.CurrentHunger < hungerCost || player.CurrentThirst < thirstCost ||
                    player.CurrentHp < card.HpCost || player.CurrentSanity < card.SanityCost)
                {
                    message = "點數不足，無法打出此裝備卸載卡牌！";
                    return PlayResult.InsufficientPoints;
                }

                player.CurrentHunger -= hungerCost;
                player.CurrentThirst -= thirstCost;
                player.CurrentHp -= card.HpCost;
                player.CurrentSanity -= card.SanityCost;

                deck.UnequipCard(parentCard, card);
                message = $"卸下了裝備【{parentCard.CardName}】。原卡已回到手牌，卸載卡已銷毀。";
                return PlayResult.Unequipped;
            }

            // 7. 特殊卡牌生效前 CanPlay 檢驗 (透過 Strategy)
            if (_effects.TryGetValue(card.CardName, out var strategy))
            {
                if (!strategy.CanPlay(card, player, deck, out message))
                {
                    return PlayResult.InvalidCard;
                }
            }

            // 8. 一般卡牌成本檢驗與扣減
            if (player.CurrentHunger < hungerCost || player.CurrentThirst < thirstCost ||
                player.CurrentHp < card.HpCost || player.CurrentSanity < card.SanityCost)
            {
                message = "點數不足，無法打出此卡牌！";
                return PlayResult.InsufficientPoints;
            }

            player.CurrentHunger -= hungerCost;
            player.CurrentThirst -= thirstCost;
            player.CurrentHp -= card.HpCost;
            player.CurrentSanity -= card.SanityCost;

            if (card.BrutalityChange != 0) player.Brutality = Math.Clamp(player.Brutality + card.BrutalityChange, 0, 100);
            if (card.CorruptionChange != 0) player.Corruption = Math.Clamp(player.Corruption + card.CorruptionChange, 0, 100);
            if (card.EvilChange != 0) player.Evil = Math.Clamp(player.Evil + card.EvilChange, 0, 100);

            // 9. 執行特定卡牌 Strategy (若有註冊)
            if (strategy != null)
            {
                return strategy.Execute(card, player, deck, out message);
            }

            // 10. 方案 3：通用/純數值卡牌邏輯 (Fallback)
            if (card.MaxUses > 0)
            {
                if (card.CardType == CardType.Consumable)
                {
                    card.UsesLeft--;
                    if (card.UsesLeft > 0)
                    {
                        deck.DiscardCard(card);
                        message = $"使用了【{card.CardName}】（剩餘次數: {card.UsesLeft}/{card.MaxUses}）。";
                    }
                    else
                    {
                        deck.Hand.Remove(card); 
                        message = $"使用了【{card.CardName}】，此物品已完全耗盡！";
                    }
                    return PlayResult.Success;
                }
            }

            int str = CardQueryHelper.GetModifiedStrValue(card, deck.Hand);
            int dex = card.DexValue;
            int wis = card.WisValue;

            if (GameState.Instance.StimulantActive)
            {
                if (card.CardType == CardType.ActionStr)
                {
                    str += 2;
                    GameState.Instance.AddLog("【興奮劑作用】激發潛能，此卡力量 +2！");
                }
                else if (card.CardType == CardType.ActionDex)
                {
                    dex += 2;
                    GameState.Instance.AddLog("【興奮劑作用】激發潛能，此卡靈巧 +2！");
                }
                else if (card.CardType == CardType.ActionWis)
                {
                    wis += 2;
                    GameState.Instance.AddLog("【興奮劑作用】激發潛能，此卡智慧 +2！");
                }
            }

            // 李曉琳 (Celin) 病嬌反噬與雙倍力量被動
            if (player.CharacterData?.CharacterId == CharacterId.Celin)
            {
                bool hasInjuryInHand = deck.Hand.Any(c => c.CardType == CardType.Injury || c.CardId == CardId.InjuryFatigue);
                if (hasInjuryInHand && (card.CardType == CardType.ActionStr || str > 0))
                {
                    str *= 2;
                    GameState.Instance.AddLog("【病嬌執念】李曉琳利用身上的傷痛使爆發力量翻倍！");
                }

                if (card.CardType == CardType.ActionStr || str > 0)
                {
                    if (new Random().NextDouble() < 0.25)
                    {
                        var fatigue = CardFactory.CreateCard(CardId.InjuryFatigue);
                        if (fatigue != null)
                        {
                            deck.AddCardToDiscardPile(fatigue);
                            GameState.Instance.AddLog("【病嬌反噬】過度用勁使李曉琳的身體累積了疲勞（獲得【疲勞】並放入棄牌堆）。");
                        }
                    }
                }
            }

            // 于晞 (Nancy) 自虐意志被動
            if (player.CharacterData?.CharacterName == "于晞" && player.CurrentHp > 20)
            {
                if (card.CardType == CardType.ActionStr || card.CardType == CardType.ActionDex || card.CardType == CardType.ActionWis)
                {
                    player.CurrentHp -= 3;
                    if (str > 0) str += 2;
                    if (dex > 0) dex += 2;
                    if (wis > 0) wis += 2;
                    GameState.Instance.AddLog("【自虐意志】于晞消耗 3 點體力刺激潛能，使該卡點數 +2！");
                }
            }

            if (card.CardType == CardType.ActionStr || card.CardType == CardType.ActionDex || card.CardType == CardType.ActionWis)
            {
                TurnManager.Instance.AccumulatedStr += str;
                TurnManager.Instance.AccumulatedDex += dex;
                TurnManager.Instance.AccumulatedWis += wis;
            }

            if (GameState.Instance.CopyNextCard && card.CardId != CardId.ActionCopy)
            {
                GameState.Instance.CopyNextCard = false;
                if (card.CardType == CardType.ActionStr || card.CardType == CardType.ActionDex || card.CardType == CardType.ActionWis)
                {
                    TurnManager.Instance.AccumulatedStr += str;
                    TurnManager.Instance.AccumulatedDex += dex;
                    TurnManager.Instance.AccumulatedWis += wis;
                }
                if (strategy != null)
                {
                    string copyMsg;
                    strategy.Execute(card, player, deck, out copyMsg);
                    GameState.Instance.AddLog($"【複製效果】再次觸發【{card.CardName}】的效果！ {copyMsg}");
                }
                else
                {
                    GameState.Instance.AddLog($"【複製效果】再次獲得屬性：力量+{str}，靈巧+{dex}，智慧+{wis}。");
                }
            }

            if (card.CardType == CardType.Equipment)
            {
                deck.EquipCard(card);
                message = $"裝備了 {card.CardName}。已從手牌移出，卸載卡已加入棄牌堆。";
            }
            else if (card.CardType == CardType.Consumable)
            {
                deck.Hand.Remove(card); 
                message = $"使用了消耗品【{card.CardName}】。";
            }
            else
            {
                deck.DiscardCard(card);
                message = $"打出了 {card.CardName}。力量+{str}，靈巧+{card.DexValue}，智慧+{card.WisValue}。";
            }

            return PlayResult.Success;
        }
    }
}
