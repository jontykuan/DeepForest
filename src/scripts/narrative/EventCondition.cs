using Godot;
using System;
using System.Linq;
using DeepForest.Character;
using DeepForest.Cards;
using DeepForest.Core;

namespace DeepForest.Narrative
{
    public partial class EventCondition : Resource
    {
        public virtual bool Evaluate(Player player, Deck deck)
        {
            return true;
        }
    }

    public partial class ConditionComposite : EventCondition
    {
        [Export] public Godot.Collections.Array<EventCondition> Conditions { get; set; } = new();
        [Export] public bool IsOrGate { get; set; } = false;

        public override bool Evaluate(Player player, Deck deck)
        {
            if (Conditions == null || Conditions.Count == 0) return true;

            if (IsOrGate)
            {
                foreach (var cond in Conditions)
                {
                    if (cond != null && cond.Evaluate(player, deck)) return true;
                }
                return false;
            }
            else
            {
                foreach (var cond in Conditions)
                {
                    if (cond != null && !cond.Evaluate(player, deck)) return false;
                }
                return true;
            }
        }
    }

    public partial class ConditionCardLocation : EventCondition
    {
        [Export] public string TargetCardId { get; set; } = "";
        [Export] public string Location { get; set; } = "Hand"; // Hand, DrawPile, DiscardPile, Equipped

        public override bool Evaluate(Player player, Deck deck)
        {
            if (deck == null || string.IsNullOrEmpty(TargetCardId)) return false;

            if (!Enum.TryParse<CardId>(TargetCardId, out var targetId))
            {
                GD.PrintErr($"[ConditionCardLocation] 無法解析 CardId: {TargetCardId}");
                return false;
            }

            return Location switch
            {
                "Hand" => deck.Hand.Any(c => c != null && c.CardId == targetId),
                "DrawPile" => deck.DrawPile.Any(c => c != null && c.CardId == targetId),
                "DiscardPile" => deck.DiscardPile.Any(c => c != null && c.CardId == targetId),
                "Equipped" => deck.EquippedCards.Any(c => c != null && c.CardId == targetId),
                "Anywhere" => CardQueryHelper.HasCardAnywhere(deck, targetId),
                _ => false
            };
        }
    }

    public partial class ConditionStatCheck : EventCondition
    {
        [Export] public string StatName { get; set; } = "Hp"; // Hp, Sanity, Hunger, Thirst, Brutality, Corruption, Evil, Day, Depth
        [Export] public string Operator { get; set; } = ">="; // <, >, <=, >=, ==
        [Export] public int Value { get; set; } = 0;

        public override bool Evaluate(Player player, Deck deck)
        {
            if (player == null) return false;

            int statValue = StatName.ToLower() switch
            {
                "hp" => player.CurrentHp,
                "sanity" => player.CurrentSanity,
                "hunger" => player.CurrentHunger,
                "thirst" => player.CurrentThirst,
                "brutality" => player.Brutality,
                "corruption" => player.Corruption,
                "evil" => player.Evil,
                "day" => GameState.Instance?.CurrentDay ?? 1,
                "depth" => GameState.Instance?.CurrentDepth ?? 0,
                _ => 0
            };

            return Operator switch
            {
                "<" => statValue < Value,
                ">" => statValue > Value,
                "<=" => statValue <= Value,
                ">=" => statValue >= Value,
                "==" => statValue == Value,
                _ => false
            };
        }
    }

    public partial class ConditionLogCheck : EventCondition
    {
        [Export] public string ActionType { get; set; } = "";
        [Export] public string ParamKey { get; set; } = "";
        [Export] public string ParamValue { get; set; } = "";
        [Export] public string Scope { get; set; } = "ThisGame"; // ThisScene, ThisTurn, ThisGame

        public override bool Evaluate(Player player, Deck deck)
        {
            var logger = GameState.Instance?.Logger;
            if (logger == null) return false;

            return logger.HasLog(ActionType, ParamKey, ParamValue, Scope);
        }
    }
}
