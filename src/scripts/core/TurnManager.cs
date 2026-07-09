using Godot;
using System;
using DeepForest.Character;
using DeepForest.Cards;
using DeepForest.Scene;

namespace DeepForest.Core;

public enum TurnPhase
{
    Start,
    Draw,
    Play,
    Clean,
    Resolve
}

public partial class TurnManager : Node
{
    public static TurnManager Instance { get; set; } = null!;

    [Signal] public delegate void PhaseChangedEventHandler(int newPhase);
    [Signal] public delegate void TurnEndedEventHandler();

    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Start;
    
    public int AccumulatedStr { get; set; } = 0;
    public int AccumulatedDex { get; set; } = 0;
    public int AccumulatedWis { get; set; } = 0;

    public override void _Ready()
    {
        Instance = this;
    }

    public void StartTurn()
    {
        CurrentPhase = TurnPhase.Start;
        EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

        AccumulatedStr = 0;
        AccumulatedDex = 0;
        AccumulatedWis = 0;
        if (GameState.Instance != null)
        {
            GameState.Instance.StimulantActive = false;
        }

        Player player = GameState.Instance.PlayerInstance;
        Deck deck = GameState.Instance.DeckInstance;

        // Discard all remaining hand cards on scene transition (turn change)
        if (deck.Hand.Count > 0)
        {
            var toDiscard = new System.Collections.Generic.List<Card>(deck.Hand);
            foreach (var c in toDiscard)
            {
                deck.Hand.Remove(c);
                if (!c.HasMeta("temporary"))
                {
                    deck.DiscardPile.Add(c);
                }
                GameState.Instance.AddLog($"捨棄未用手牌: {c.CardName}");
            }
            deck.EmitSignal(Deck.SignalName.HandChanged);
            deck.EmitSignal(Deck.SignalName.DeckChanged);
        }

        ResolveStartOfTurnEffects(player, deck);
        DrawPhase();
    }

    private void ResolveStartOfTurnEffects(Player player, Deck deck)
    {
        if (player.CurrentHunger <= 0)
        {
            player.CurrentHp -= 5;
            player.CurrentSanity -= 5;
            GameState.Instance.AddLog("因為極度飢餓，你的體力與理智都在流失。");
        }

        if (player.CurrentThirst <= 0)
        {
            player.CurrentHp -= 5;
            player.CurrentSanity -= 5;
            GameState.Instance.AddLog("因為極度口渴，你的體力與理智都在流失。");
        }

        if (StatusEffect.HasEffect(deck.Hand, CardEffectTag.WoundInfection))
        {
            player.CurrentHp -= 3;
            GameState.Instance.AddLog("傷口感染惡化，扣減 3 點體力。");
        }

        if (StatusEffect.HasEffect(deck.Hand, CardEffectTag.Corruption))
        {
            player.CurrentSanity -= 5;
            GameState.Instance.AddLog("手牌中的【穢祟附身】發揮邪祟效果，扣減了你 5 點理智。");
        }

        if (deck.Hand.Any(c => c.CardId == CardId.KeyJerryQuestion))
        {
            player.Corruption += 1;
            GameState.Instance.AddLog("手牌中的【傑利？】散發著詭異不祥的氣息，使你的穢祟加深（穢祟 +1）。");
        }

        // --- NEW STORIES/EVENTS PASSIVE CHECKS ---
        if (GameState.Instance != null)
        {
            if (GameState.Instance.IsDescentActive)
            {
                player.CurrentSanity -= 5;
                GameState.Instance.AddLog("在降神的邪氣侵蝕下，你的理智在持續流失（理智 -5）。");
            }

            if (player.CharacterData?.CharacterId == CharacterId.Nancy && player.CurrentSanity < 20)
            {
                if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRecordingTape))
                {
                    CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeyRecordingTape);
                    GameState.Instance.AddLog("【丟失卡牌】由于你的理智低於 20，你在恍惚中遺失了【錄音帶】！");
                }
            }

            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyLockedDiary) && CardQueryHelper.HasCardAnywhere(deck, CardId.KeyOldKey))
            {
                CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeyLockedDiary);
                CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeyOldKey);
                
                Card painfulTruth = CardFactory.CreateCard(CardId.KeyPainfulTruth);
                if (painfulTruth != null)
                {
                    deck.AddCardToDiscardPile(painfulTruth);
                    GameState.Instance.AddLog("【卡牌融合】你用老舊鑰匙打開了上鎖的日記，揭開了【痛苦真相】！");
                }
            }
        }
    }

    private void DrawPhase()
    {
        CurrentPhase = TurnPhase.Draw;
        EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

        Player player = GameState.Instance.PlayerInstance;
        Deck deck = GameState.Instance.DeckInstance;

        bool triggeredReshuffle = deck.DrawCards(player.Draw, ignoreLimit: true);
        if (triggeredReshuffle)
        {
            TriggerDayChange();
        }

        PlayPhase();
    }

    private void PlayPhase()
    {
        CurrentPhase = TurnPhase.Play;
        EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
        SessionSaveSystem.SaveSession(GameState.Instance, MapManager.Instance);
    }

    public void EndPlayPhase()
    {
        CleanPhase();
    }

    private void CleanPhase()
    {
        CurrentPhase = TurnPhase.Clean;
        EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

        Player player = GameState.Instance.PlayerInstance;
        Deck deck = GameState.Instance.DeckInstance;

        if (deck.Hand.Count > player.HandLimit)
        {
            while (deck.Hand.Count > player.HandLimit)
            {
                Card cardToDiscard = deck.Hand[deck.Hand.Count - 1];
                deck.DiscardCard(cardToDiscard);
                GameState.Instance.AddLog($"捨棄手牌: {cardToDiscard.CardName}");
            }
        }

        ResolvePhase();
    }

    private void ResolvePhase()
    {
        CurrentPhase = TurnPhase.Resolve;
        EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
        EmitSignal(SignalName.TurnEnded);
    }

    public void TriggerDayChange()
    {
        Player player = GameState.Instance.PlayerInstance;
        Deck deck = GameState.Instance.DeckInstance;
        EnvironmentSystem env = EnvironmentSystem.Instance;
        var balance = GD.Load<BalanceData>("res://src/resources/balance/balance_data.tres");

        GameState.Instance.CurrentDay += 1;
        GameState.Instance.AddLog($"夜幕降臨。度過第 {GameState.Instance.CurrentDay} 天。");

        // Overload fatigue check on day change
        if (deck.GetTotalWeight() > player.DeckCapacity)
        {
            Card fatigue = CardFactory.CreateCard(CardId.InjuryFatigue);
            if (fatigue != null)
            {
                deck.AddCardToDiscardPile(fatigue);
                GameState.Instance.AddLog("【超重負載】換日結算：由於背包超重，你感到極度疲憊，牌組中被加入了【疲勞】卡。");
            }
        }

        int day = GameState.Instance.CurrentDay;
        int depth = GameState.Instance.CurrentDepth;
        
        int baseHungerLoss = (balance != null ? balance.HungerDepletion : 3) + (day / 4) + (depth / 10);
        int baseThirstLoss = (balance != null ? balance.ThirstDepletion : 3) + (day / 4) + (depth / 10);

        player.CurrentHunger -= baseHungerLoss;
        player.CurrentThirst -= baseThirstLoss;
        GameState.Instance.AddLog($"今日消耗：飢餓值 -{baseHungerLoss}，口渴值 -{baseThirstLoss}。");

        int hpRecovery = balance != null ? balance.BaseHpRecovery : 8;
        int sanityRecovery = balance != null ? balance.BaseSanityRecovery : 5;

        float recoveryMult = env != null ? env.GetRestRecoveryMultiplier() : 1.0f;
        hpRecovery = (int)(hpRecovery * recoveryMult);
        sanityRecovery = (int)(sanityRecovery * recoveryMult);

        player.CurrentHp += hpRecovery;
        player.CurrentSanity += sanityRecovery;
        player.CurrentHp -= 5; 
        
        GameState.Instance.AddLog($"休息恢復：體力 +{hpRecovery} (含寒冷/天候扣減)，理智 +{sanityRecovery}。");

        if (env != null)
        {
            env.GenerateRandomEnvironment();
            GameState.Instance.AddLog($"今天的天氣: {env.GetWeatherString()} | 溫度: {env.GetTempString()} | 濕度: {env.GetHumidityString()}");
        }

        // --- DAILY STORIES/EVENTS CHECKS ---
        if (GameState.Instance != null && deck != null)
        {
            // 1. Sarah baby son curse check
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyBabySon))
            {
                CardId[] curses = { CardId.InjuryFatigue, CardId.CurseAddiction };
                CardId chosen = curses[new Random().Next(curses.Length)];
                Card curse = CardFactory.CreateCard(chosen);
                if (curse != null)
                {
                    deck.AddCardToDiscardPile(curse);
                    GameState.Instance.AddLog($"【寶貝兒子的負擔】牌組中被加入了【{curse.CardName}】。");
                }
            }

            // 2. Leo Celin stalking check
            if (player.CharacterData?.CharacterId == CharacterId.Leo && GameState.Instance.CelinStalkDay > 0)
            {
                if (GameState.Instance.CurrentDay >= GameState.Instance.CelinStalkDay)
                {
                    GameState.Instance.AddLog("【尾隨襲擊】李曉琳在黑暗中現身，眼神中充滿了病態的狂熱！");
                    GameState.Instance.CelinStalkDay = GameState.Instance.CurrentDay + 3;
                    DeepForest.Narrative.EventManager.HandleNpcEncounter(player, deck, "Celin");
                }
            }

            // 3. Addiction check: if CurseAddiction is in hand during day change, add another one
            int addictionInHand = deck.Hand.Count(c => c.CardId == CardId.CurseAddiction);
            if (addictionInHand > 0)
            {
                for (int i = 0; i < addictionInHand; i++)
                {
                    Card addictionCard = CardFactory.CreateCard(CardId.CurseAddiction);
                    if (addictionCard != null)
                    {
                        deck.AddCardToDiscardPile(addictionCard);
                    }
                }
                GameState.Instance.AddLog($"【毒癮發作】手牌中的 {addictionInHand} 張【成癮】產生了啃食，額外將 {addictionInHand} 張【成癮】卡加入了棄牌堆。");
            }
        }

        SessionSaveSystem.SaveSession(GameState.Instance, MapManager.Instance);
    }
}
