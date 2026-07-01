using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Cards;
using DeepForest.Core;
using DeepForest.Character;
using DeepForest.Scene;

namespace DeepForest.Combat;

public class CombatManager
{
    private static CombatManager? _instance;
    public static CombatManager Instance => _instance ??= new CombatManager();

    // 暫存被擊敗敵人的掉落物，方便戰後搜屍/解剖時獲取
    public EnemyData? LastDefeatedEnemy { get; private set; }

    public void StartCombat(EnemyData enemyData)
    {
        var state = GameState.Instance;
        state.IsInCombat = true;
        state.CurrentEnemy = enemyData;
        state.CurrentEnemyHp = enemyData.MaxHp;
        state.CombatPlayedCards.Clear();
        state.AddLog($"【戰鬥開始】一隻【{enemyData.EnemyName}】阻擋在前方！");
    }

    public void AddCardToCombatZone(Card card)
    {
        var state = GameState.Instance;
        var deck = state.DeckInstance;

        // 只能出屬性卡進行對決
        if (card.CardType != CardType.ActionStr && card.CardType != CardType.ActionDex && card.CardType != CardType.ActionWis)
        {
            state.AddLog("只有【力量】、【靈巧】或【智慧】屬性卡能放入對決區。");
            return;
        }

        // 若已出的卡與新卡型別不同，將先前卡牌全部退回手牌
        if (state.CombatPlayedCards.Count > 0)
        {
            if (state.CombatPlayedCards[0].CardType != card.CardType)
            {
                foreach (var c in state.CombatPlayedCards)
                {
                    deck.Hand.Add(c);
                }
                state.CombatPlayedCards.Clear();
                state.AddLog("出牌型別不同，已將先前的對決卡牌退回手牌。");
            }
        }

        // 將卡牌從手牌移到對決區
        deck.Hand.Remove(card);
        state.CombatPlayedCards.Add(card);
    }

    public void RemoveCardFromCombatZone(Card card)
    {
        var state = GameState.Instance;
        var deck = state.DeckInstance;

        if (state.CombatPlayedCards.Remove(card))
        {
            deck.Hand.Add(card);
        }
    }

    public void ResolveClash()
    {
        var state = GameState.Instance;
        var enemy = state.CurrentEnemy;
        if (enemy == null) return;

        // 1. 敵方隨機出牌
        Card enemyCard;
        if (enemy.ActionDeck.Count > 0)
        {
            int randIdx = new Random().Next(enemy.ActionDeck.Count);
            enemyCard = enemy.ActionDeck[randIdx];
        }
        else
        {
            enemyCard = new Card { CardName = "野性撕咬", CardType = CardType.ActionStr, StrValue = 2 };
        }

        int enemyVal = GetCardValue(enemyCard);
        CardType enemyType = enemyCard.CardType;

        // 2. 玩家未出牌，視同失敗
        if (state.CombatPlayedCards.Count == 0)
        {
            state.PlayerInstance.CurrentHp -= enemyVal;
            state.AddLog($"你沒有出牌對決！【{enemy.EnemyName}】使出了【{enemyCard.CardName}】（{GetTypeName(enemyType)} {enemyVal} 點），你受到了 {enemyVal} 點體力傷害！");
            CheckCombatEnd();
            return;
        }

        CardType playerType = state.CombatPlayedCards[0].CardType;
        int playerVal = 0;
        foreach (var c in state.CombatPlayedCards)
        {
            playerVal += GetCardValue(c);
        }

        // 裝備加成 (柴刀：力量 +1)
        bool hasMachete = deckHasEquipped(state.DeckInstance, "柴刀");
        if (playerType == CardType.ActionStr && hasMachete)
        {
            playerVal += 1;
        }

        // 3. 剪刀石頭布剋制與點數比拼
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

        // 4. 結算結果
        string playerTypeDesc = GetTypeName(playerType);
        string enemyTypeDesc = GetTypeName(enemyType);

        if (playerWin)
        {
            state.CurrentEnemyHp--;
            state.AddLog($"【對決勝利】你出 {playerTypeDesc}（共 {playerVal} 點），敵出 {enemyTypeDesc}（{enemyVal} 點）。你對【{enemy.EnemyName}】造成了 1 點傷害！");
        }
        else if (enemyWin)
        {
            state.PlayerInstance.CurrentHp -= enemyVal;
            state.AddLog($"【對決失敗】你出 {playerTypeDesc}（共 {playerVal} 點），敵出 {enemyTypeDesc}（{enemyVal} 點）。你受到了 {enemyVal} 點體力傷害！");
        }
        else
        {
            state.AddLog($"【平手】雙方均出 {playerTypeDesc}，點數同為 {playerVal} 點。相安無事。");
        }

        // 5. 棄置玩家出牌
        foreach (var c in state.CombatPlayedCards)
        {
            state.DeckInstance.DiscardPile.Add(c);
        }
        state.CombatPlayedCards.Clear();

        CheckCombatEnd();
    }

    private void CheckCombatEnd()
    {
        var state = GameState.Instance;
        var enemy = state.CurrentEnemy;
        if (enemy == null) return;

        if (state.CurrentEnemyHp <= 0)
        {
            state.AddLog($"【戰鬥勝利】你擊敗了【{enemy.EnemyName}】！");
            LastDefeatedEnemy = enemy;
            state.IsInCombat = false;
            TriggerPostCombatScene();
        }
        else if (state.PlayerInstance.CurrentHp <= 0)
        {
            state.IsInCombat = false;
            state.AddLog("【你倒下了】你的體力已耗盡... 森林深處的黑暗將你吞噬。");
            state.EndingManagerInstance.CheckEndGameConditions();
        }
    }

    private void TriggerPostCombatScene()
    {
        var state = GameState.Instance;
        var enemy = state.CurrentEnemy;
        if (enemy == null) return;

        var mapManager = MapManager.Instance;
        var currentNode = mapManager.Nodes[mapManager.CurrentNodeId];
        
        // 清空原場景行動，換成戰後選項
        currentNode.SceneData.Actions.Clear();
        
        if (enemy.LootTable.Count > 0)
        {
            currentNode.SceneData.Actions.Add(new SceneAction { 
                ActionName = $"搜屍 ({enemy.EnemyName})", 
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

        state.CurrentEnemy = null;
    }

    private int GetCardValue(Card c)
    {
        return c.StrValue + c.DexValue + c.WisValue;
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

    private bool deckHasEquipped(Deck deck, string cardName)
    {
        return deck.EquippedCards.Exists(c => c.CardName == cardName);
    }
}
