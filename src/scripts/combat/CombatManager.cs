using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DeepForest.Cards;
using DeepForest.Core;
using DeepForest.Character;
using DeepForest.Scene;
using DeepForest.Narrative;

namespace DeepForest.Combat
{
    public partial class CombatManager : Node
    {
        public static CombatManager Instance { get; set; } = null!;

        [Signal] public delegate void CombatStartedEventHandler(EnemyData enemy);
        [Signal] public delegate void CombatEndedEventHandler(bool victory);
        [Signal] public delegate void CombatStateChangedEventHandler();

        public bool IsInCombat { get; set; } = false;
        public EnemyData? CurrentEnemy { get; set; } = null;
        public int CurrentEnemyHp { get; set; } = 0;
        public List<Card> CombatPlayedCards { get; private set; } = new();

        public EnemyData? LastDefeatedEnemy { get; private set; }

        public int CurrentRound { get; set; } = 1;
        public int TotalRounds { get; set; } = 0;
        public int PlayerWinsCount { get; set; } = 0;
        public int EnemyWinsCount { get; set; } = 0;

        public override void _EnterTree()
        {
            Instance = this;
        }

        public void StartCombat(EnemyData enemyData)
        {
            IsInCombat = true;
            CurrentEnemy = enemyData;
            CurrentEnemyHp = enemyData.MaxHp;

            TotalRounds = enemyData.MaxHp;
            CurrentRound = 1;
            PlayerWinsCount = 0;
            EnemyWinsCount = 0;

            CombatPlayedCards.Clear();
            GameState.Instance.AddLog($"【戰鬥開始】一隻【{enemyData.EnemyName}】阻擋在前方！（進行 {TotalRounds} 輪對決，需獲勝過半輪數以擊敗對手）");
            EmitSignal(SignalName.CombatStarted, enemyData);
            EmitSignal(SignalName.CombatStateChanged);
        }

        public void AddCardToCombatZone(Card card)
        {
            var state = GameState.Instance;
            var deck = state.DeckInstance;

            if (card.CardType != CardType.ActionStr && card.CardType != CardType.ActionDex && card.CardType != CardType.ActionWis)
            {
                state.AddLog("只有【力量】、【靈巧】或【智慧】屬性卡能放入對決區。");
                return;
            }

            if (CombatPlayedCards.Count > 0)
            {
                if (CombatPlayedCards[0].CardType != card.CardType)
                {
                    foreach (var c in CombatPlayedCards)
                    {
                        deck.Hand.Add(c);
                    }
                    CombatPlayedCards.Clear();
                    state.AddLog("出牌型別不同，已將先前的對決卡牌退回手牌。");
                }
            }

            deck.Hand.Remove(card);
            CombatPlayedCards.Add(card);
            EmitSignal(SignalName.CombatStateChanged);
        }

        public void RemoveCardFromCombatZone(Card card)
        {
            var deck = GameState.Instance.DeckInstance;
            if (CombatPlayedCards.Remove(card))
            {
                deck.Hand.Add(card);
                EmitSignal(SignalName.CombatStateChanged);
            }
        }

        public void ResolveClash()
        {
            var state = GameState.Instance;
            var enemy = CurrentEnemy;
            if (enemy == null) return;

            Card enemyCard;
            if (enemy.ActionDeck.Count > 0)
            {
                int randIdx = Random.Shared.Next(enemy.ActionDeck.Count);
                enemyCard = enemy.ActionDeck[randIdx];
            }
            else
            {
                enemyCard = new Card { CardName = "野性撕咬", CardType = CardType.ActionStr, StrValue = 2 };
            }

            int enemyVal = GetCardValue(enemyCard);
            CardType enemyType = enemyCard.CardType;
            string enemyTypeDesc = GetTypeName(enemyType);

            if (CombatPlayedCards.Count == 0)
            {
                EnemyWinsCount++;
                if (enemyType == CardType.ActionWis || enemy.EnemyName == "怨靈")
                {
                    state.PlayerInstance.CurrentSanity -= enemyVal;
                    state.AddLog($"【對決失敗】第 {CurrentRound} 輪你沒有出牌對決！【{enemy.EnemyName}】使出了【{enemyCard.CardName}】（{enemyTypeDesc} {enemyVal} 點），你受到了 {enemyVal} 點理智傷害！");
                }
                else
                {
                    state.PlayerInstance.CurrentHp -= enemyVal;
                    state.AddLog($"【對決失敗】第 {CurrentRound} 輪你沒有出牌對決！【{enemy.EnemyName}】使出了【{enemyCard.CardName}】（{enemyTypeDesc} {enemyVal} 點），你受到了 {enemyVal} 點體力傷害！");
                }
                CurrentRound++;
                CurrentEnemyHp = Math.Max(0, TotalRounds - CurrentRound + 1);
                EmitSignal(SignalName.CombatStateChanged);
                CheckCombatEnd();
                return;
            }

            CardType playerType = CombatPlayedCards[0].CardType;
            int playerVal = 0;
            foreach (var c in CombatPlayedCards)
            {
                playerVal += GetCardValue(c);
            }

            bool hasMachete = deckHasEquipped(state.DeckInstance, CardId.EquipmentKnife);
            if (playerType == CardType.ActionStr && hasMachete)
            {
                playerVal += 1;
            }

            bool playerWin = false;
            bool enemyWin = false;

            if (playerType == enemyType)
            {
                if (playerVal > enemyVal) playerWin = true;
                else if (playerVal < enemyVal) enemyWin = true;
            }
            else
            {
                if (playerType == CardType.ActionStr && enemyType == CardType.ActionWis) playerWin = true;
                else if (playerType == CardType.ActionWis && enemyType == CardType.ActionDex) playerWin = true;
                else if (playerType == CardType.ActionDex && enemyType == CardType.ActionStr) playerWin = true;
                else enemyWin = true;
            }

            string playerTypeDesc = GetTypeName(playerType);

            if (playerWin)
            {
                PlayerWinsCount++;
                state.AddLog($"【對決勝利】第 {CurrentRound} 輪勝利！你出 {playerTypeDesc}（共 {playerVal} 點），敵出 {enemyTypeDesc}（{enemyVal} 點）。");
            }
            else if (enemyWin)
            {
                EnemyWinsCount++;
                if (enemyType == CardType.ActionWis || enemy.EnemyName == "怨靈")
                {
                    state.PlayerInstance.CurrentSanity -= enemyVal;
                    state.AddLog($"【對決失敗】第 {CurrentRound} 輪失敗！你出 {playerTypeDesc}（共 {playerVal} 點），敵出 {enemyTypeDesc}（{enemyVal} 點）。你受到了 {enemyVal} 點理智傷害！");
                }
                else
                {
                    state.PlayerInstance.CurrentHp -= enemyVal;
                    state.AddLog($"【對決失敗】第 {CurrentRound} 輪失敗！你出 {playerTypeDesc}（共 {playerVal} 點），敵出 {enemyTypeDesc}（{enemyVal} 點）。你受到了 {enemyVal} 點體力傷害！");
                }
            }
            else
            {
                state.AddLog($"【對決平手】第 {CurrentRound} 輪平手！雙方均出 {playerTypeDesc}，點數同為 {playerVal} 點。相安無事。");
            }

            foreach (var c in CombatPlayedCards)
            {
                state.DeckInstance.DiscardPile.Add(c);
            }
            CombatPlayedCards.Clear();

            CurrentRound++;
            CurrentEnemyHp = Math.Max(0, TotalRounds - CurrentRound + 1);
            EmitSignal(SignalName.CombatStateChanged);

            CheckCombatEnd();
        }

        private void CheckCombatEnd()
        {
            var state = GameState.Instance;
            var enemy = CurrentEnemy;
            if (enemy == null) return;

            // Immediate collapse check
            if (state.PlayerInstance.CurrentHp <= 0 || state.PlayerInstance.CurrentSanity <= 0)
            {
                IsInCombat = false;
                EmitSignal(SignalName.CombatEnded, false);
                state.AddLog("【你倒下了】你在戰鬥中失去了意識... 森林深處的黑暗將你吞噬。");
                state.EndingManagerInstance.CheckEndGameConditions();
                return;
            }

            // Fixed-round completed check
            if (CurrentRound > TotalRounds)
            {
                if (PlayerWinsCount > EnemyWinsCount)
                {
                    state.AddLog($"【對決結果】戰鬥勝利！你贏得了大多數輪次（贏 {PlayerWinsCount} 輪，輸 {EnemyWinsCount} 輪，總共 {TotalRounds} 輪）。你成功擊退了【{enemy.EnemyName}】！");
                    LastDefeatedEnemy = enemy;
                    IsInCombat = false;
                    EmitSignal(SignalName.CombatEnded, true);
                    TriggerPostCombatScene(true);
                }
                else
                {
                    state.AddLog($"【對決結果】戰鬥失敗！你未能贏得足夠輪次（贏 {PlayerWinsCount} 輪，輸 {EnemyWinsCount} 輪，總共 {TotalRounds} 輪）。");
                    IsInCombat = false;
                    EmitSignal(SignalName.CombatEnded, false);

                    if (enemy.EnemyName == "母親的幻影")
                    {
                        state.PlayerInstance.CurrentHp = 10;
                        CardQueryHelper.RemoveCardAnywhere(state.DeckInstance, CardId.KeyJerryCollar);
                        SaveManager.UnlockEnding("湯明亮", "成就：再見了摯友");
                        StoryUnlock.Instance?.UnlockStorySegment("成就：再見了摯友", "在與母親幻影的爭奪中，你失去了傑利的項圈... 與過去徹底告別。");
                        state.AddLog("【戰鬥失敗】母親的幻影無情地將你擊倒，並奪走了你脖子上的【傑利項圈】！你在冷汗中醒來...（解鎖成就：再見了摯友）");
                    }
                    else
                    {
                        if (state.CurrentStoryHandler == null || !state.CurrentStoryHandler.HandleSpecialEvent(state.PlayerInstance, state.DeckInstance, $"combat_failure_{enemy.EnemyName}", 1))
                        {
                            state.AddLog($"【對決失敗】你被【{enemy.EnemyName}】擊敗，只能狼狽退開。");
                        }
                    }
                    TriggerPostCombatScene(false);
                }
            }
        }

        private void TriggerPostCombatScene(bool victory)
        {
            var state = GameState.Instance;
            var enemy = CurrentEnemy;
            if (enemy == null) return;

            var mapManager = MapManager.Instance;
            if (mapManager != null && mapManager.Nodes != null && mapManager.Nodes.ContainsKey(mapManager.CurrentNodeId))
            {
                var currentNode = mapManager.Nodes[mapManager.CurrentNodeId];
                if (currentNode != null && currentNode.SceneData != null)
                {
                    currentNode.SceneData.Actions.Clear();

                    for (int i = currentNode.SceneData.Decals.Count - 1; i >= 0; i--)
                    {
                        if (currentNode.SceneData.Decals[i].StartsWith("combat_"))
                        {
                            currentNode.SceneData.Decals.RemoveAt(i);
                        }
                    }
                    
                    if (victory && enemy.LootTable.Count > 0)
                    {
                        currentNode.SceneData.Actions.Add(new SceneAction { 
                            ActionName = $"拾取遺物 ({enemy.EnemyName})", 
                            ThresholdType = ThresholdType.None, 
                            ThresholdValue = 0, 
                            EffectType = ActionEffectType.LootCorpse 
                        });
                    }

                    currentNode.SceneData.Actions.Add(new SceneAction { 
                        ActionName = "繼續前進", 
                        ThresholdType = ThresholdType.Dex, 
                        ThresholdValue = 2, 
                        EffectType = ActionEffectType.MoveForward 
                    });
                }
            }

            CurrentEnemy = null;
        }

        public int GetCardValue(Card c)
        {
            var deck = GameState.Instance.DeckInstance;
            int str = CardQueryHelper.GetModifiedStrValue(c, deck.Hand);
            return str + c.DexValue + c.WisValue;
        }

        private string GetTypeName(CardType type)
        {
            return type switch
            {
                CardType.ActionStr => "力量",
                CardType.ActionDex => "靈巧",
                CardType.ActionWis => "智慧",
                _ => "未知"
            };
        }

        private bool deckHasEquipped(Deck deck, CardId id)
        {
            return deck.EquippedCards.Exists(c => c.CardId == id);
        }
    }
}
