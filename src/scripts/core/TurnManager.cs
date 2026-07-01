using Godot;
using System;
using DeepForest.Character;
using DeepForest.Cards;

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
    public static TurnManager Instance { get; private set; } = null!;

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

        Player player = GameState.Instance.PlayerInstance;
        Deck deck = GameState.Instance.DeckInstance;

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

        if (StatusEffect.HasEffect(deck.Hand, "傷口感染"))
        {
            player.CurrentHp -= 3;
            GameState.Instance.AddLog("傷口感染惡化，扣減 3 點體力。");
        }

        if (StatusEffect.HasEffect(deck.Hand, "穢祟附身"))
        {
            player.CurrentSanity -= 5;
            GameState.Instance.AddLog("手牌中的【穢祟附身】發揮邪祟效果，扣減了你 5 點理智。");
        }
    }

    private void DrawPhase()
    {
        CurrentPhase = TurnPhase.Draw;
        EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

        Player player = GameState.Instance.PlayerInstance;
        Deck deck = GameState.Instance.DeckInstance;

        bool triggeredReshuffle = deck.DrawCards(player.Draw);
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
        EnvironmentSystem env = EnvironmentSystem.Instance;

        GameState.Instance.CurrentDay += 1;
        GameState.Instance.AddLog($"夜幕降臨。度過第 {GameState.Instance.CurrentDay} 天。");

        int day = GameState.Instance.CurrentDay;
        int depth = GameState.Instance.CurrentDepth;
        
        int baseHungerLoss = 3 + (day / 4) + (depth / 10);
        int baseThirstLoss = 3 + (day / 4) + (depth / 10);

        player.CurrentHunger -= baseHungerLoss;
        player.CurrentThirst -= baseThirstLoss;
        GameState.Instance.AddLog($"今日消耗：飢餓值 -{baseHungerLoss}，口渴值 -{baseThirstLoss}。");

        int hpRecovery = 8;
        int sanityRecovery = 5;

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
    }
}
