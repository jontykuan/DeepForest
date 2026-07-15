using Godot;
using System;
using DeepForest.Character;
using DeepForest.Cards;
using DeepForest.Core;

namespace DeepForest.Narrative
{
    public partial class EventEffect : Resource
    {
        public virtual void Execute(Player player, Deck deck)
        {
        }
    }

    public partial class EffectComposite : EventEffect
    {
        [Export] public Godot.Collections.Array<EventEffect> Effects { get; set; } = new();

        public override void Execute(Player player, Deck deck)
        {
            if (Effects == null) return;
            foreach (var effect in Effects)
            {
                effect?.Execute(player, deck);
            }
        }
    }

    public partial class EffectConditional : EventEffect
    {
        [Export] public EventCondition? Condition { get; set; }
        [Export] public Godot.Collections.Array<EventEffect> ThenEffects { get; set; } = new();
        [Export] public Godot.Collections.Array<EventEffect> ElseEffects { get; set; } = new();

        public override void Execute(Player player, Deck deck)
        {
            bool condResult = Condition == null || Condition.Evaluate(player, deck);
            
            var effectsToRun = condResult ? ThenEffects : ElseEffects;
            if (effectsToRun != null)
            {
                foreach (var effect in effectsToRun)
                {
                    effect?.Execute(player, deck);
                }
            }
        }
    }

    public partial class EffectModifyStat : EventEffect
    {
        [Export] public string StatName { get; set; } = "Hp"; // Hp, Sanity, Hunger, Thirst, Brutality, Corruption, Evil
        [Export] public int ChangeValue { get; set; } = 0;

        public override void Execute(Player player, Deck deck)
        {
            if (player == null) return;

            switch (StatName.ToLower())
            {
                case "hp":
                    player.CurrentHp += ChangeValue;
                    break;
                case "sanity":
                    player.CurrentSanity += ChangeValue;
                    break;
                case "hunger":
                    player.CurrentHunger += ChangeValue;
                    break;
                case "thirst":
                    player.CurrentThirst += ChangeValue;
                    break;
                case "brutality":
                    player.Brutality += ChangeValue;
                    break;
                case "corruption":
                    player.Corruption += ChangeValue;
                    break;
                case "evil":
                    player.Evil += ChangeValue;
                    break;
            }
        }
    }

    public partial class EffectCardTransfer : EventEffect
    {
        [Export] public string TargetCardId { get; set; } = "";
        [Export] public string Action { get; set; } = "Add"; // Add, Remove, Draw
        [Export] public int Count { get; set; } = 1;

        public override void Execute(Player player, Deck deck)
        {
            if (deck == null) return;

            if (Action == "Draw")
            {
                deck.DrawCards(Count);
                return;
            }

            if (string.IsNullOrEmpty(TargetCardId)) return;

            if (!Enum.TryParse<CardId>(TargetCardId, out var targetId))
            {
                GD.PrintErr($"[EffectCardTransfer] 無法解析 CardId: {TargetCardId}");
                return;
            }

            if (Action == "Add")
            {
                for (int i = 0; i < Count; i++)
                {
                    var card = CardFactory.CreateCard(targetId);
                    if (card != null)
                    {
                        deck.AddCardToDiscardPile(card);
                    }
                }
            }
            else if (Action == "Remove")
            {
                for (int i = 0; i < Count; i++)
                {
                    CardQueryHelper.RemoveCardAnywhere(deck, targetId);
                }
            }
        }
    }

    public partial class EffectMoveNode : EventEffect
    {
        [Export] public int TargetNodeId { get; set; }

        public override void Execute(Player player, Deck deck)
        {
            if (Scene.MapManager.Instance != null)
            {
                Scene.MapManager.Instance.CurrentNodeId = TargetNodeId;
            }
        }
    }

    public partial class EffectSetFlag : EventEffect
    {
        [Export] public string FlagName { get; set; } = ""; // IsDescentActive, NancySuicideFlag, CelinStalkingActive
        [Export] public bool FlagValue { get; set; } = false;

        public override void Execute(Player player, Deck deck)
        {
            if (GameState.Instance == null) return;

            switch (FlagName)
            {
                case "IsDescentActive":
                    GameState.Instance.IsDescentActive = FlagValue;
                    break;
                case "NancySuicideFlag":
                    GameState.Instance.NancySuicideFlag = FlagValue;
                    break;
                case "CelinStalkingActive":
                    GameState.Instance.CelinStalkingActive = FlagValue;
                    break;
            }
        }
    }
}
