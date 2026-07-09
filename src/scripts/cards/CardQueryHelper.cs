using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepForest.Cards
{
    public static class CardQueryHelper
    {
        public static bool HasCardAnywhere(Deck deck, CardId id)
        {
            if (deck == null) return false;
            return deck.DrawPile.Any(c => c.CardId == id)
                || deck.Hand.Any(c => c.CardId == id)
                || deck.DiscardPile.Any(c => c.CardId == id)
                || deck.EquippedCards.Any(c => c.CardId == id);
        }

        public static bool HasCardAnywhere(Deck deck, string cardName)
        {
            if (deck == null) return false;
            return deck.DrawPile.Any(c => c.CardName == cardName)
                || deck.Hand.Any(c => c.CardName == cardName)
                || deck.DiscardPile.Any(c => c.CardName == cardName)
                || deck.EquippedCards.Any(c => c.CardName == cardName);
        }

        public static bool HasCardAnywhere(Deck deck, CardEffectTag tag)
        {
            if (deck == null) return false;
            return deck.DrawPile.Any(c => c.EffectTags.HasFlag(tag))
                || deck.Hand.Any(c => c.EffectTags.HasFlag(tag))
                || deck.DiscardPile.Any(c => c.EffectTags.HasFlag(tag))
                || deck.EquippedCards.Any(c => c.EffectTags.HasFlag(tag));
        }

        public static Card? FindCardAnywhere(Deck deck, CardId id)
        {
            if (deck == null) return null;
            return deck.DrawPile.FirstOrDefault(c => c.CardId == id)
                ?? deck.Hand.FirstOrDefault(c => c.CardId == id)
                ?? deck.DiscardPile.FirstOrDefault(c => c.CardId == id)
                ?? deck.EquippedCards.FirstOrDefault(c => c.CardId == id);
        }

        public static Card? FindCardAnywhere(Deck deck, string cardName)
        {
            if (deck == null) return null;
            return deck.DrawPile.FirstOrDefault(c => c.CardName == cardName)
                ?? deck.Hand.FirstOrDefault(c => c.CardName == cardName)
                ?? deck.DiscardPile.FirstOrDefault(c => c.CardName == cardName)
                ?? deck.EquippedCards.FirstOrDefault(c => c.CardName == cardName);
        }

        public static int GetModifiedStrValue(Card card, IEnumerable<Card> hand)
        {
            int str = card.StrValue;
            if (str > 0)
            {
                if (Character.StatusEffect.HasBrokenArm(hand))
                {
                    str = Math.Max(1, str / 2);
                }
                int fractureCount = Character.StatusEffect.GetFractureCount(hand);
                if (fractureCount > 0)
                {
                    str = Math.Max(1, str >> fractureCount);
                }
            }
            return str;
        }

        public static bool HasCardEquipped(Deck deck, CardId id)
        {
            if (deck == null) return false;
            return deck.EquippedCards.Any(c => c.CardId == id);
        }

        public static bool HasCardEquipped(Deck deck, string cardName)
        {
            if (deck == null) return false;
            return deck.EquippedCards.Any(c => c.CardName == cardName);
        }

        public static bool RemoveCardAnywhere(Deck deck, CardId id)
        {
            if (deck == null) return false;
            bool removed = false;
            
            for (int i = deck.Hand.Count - 1; i >= 0; i--)
            {
                if (deck.Hand[i].CardId == id)
                {
                    deck.Hand.RemoveAt(i);
                    removed = true;
                }
            }
            for (int i = deck.DrawPile.Count - 1; i >= 0; i--)
            {
                if (deck.DrawPile[i].CardId == id)
                {
                    deck.DrawPile.RemoveAt(i);
                    removed = true;
                }
            }
            for (int i = deck.DiscardPile.Count - 1; i >= 0; i--)
            {
                if (deck.DiscardPile[i].CardId == id)
                {
                    deck.DiscardPile.RemoveAt(i);
                    removed = true;
                }
            }
            for (int i = deck.EquippedCards.Count - 1; i >= 0; i--)
            {
                if (deck.EquippedCards[i].CardId == id)
                {
                    deck.EquippedCards.RemoveAt(i);
                    removed = true;
                }
            }
            
            if (removed)
            {
                deck.EmitSignal(Deck.SignalName.HandChanged);
                deck.EmitSignal(Deck.SignalName.DeckChanged);
            }
            return removed;
        }

        public static bool RemoveCardAnywhere(Deck deck, string cardName)
        {
            if (deck == null) return false;
            bool removed = false;
            
            for (int i = deck.Hand.Count - 1; i >= 0; i--)
            {
                if (deck.Hand[i].CardName == cardName)
                {
                    deck.Hand.RemoveAt(i);
                    removed = true;
                }
            }
            for (int i = deck.DrawPile.Count - 1; i >= 0; i--)
            {
                if (deck.DrawPile[i].CardName == cardName)
                {
                    deck.DrawPile.RemoveAt(i);
                    removed = true;
                }
            }
            for (int i = deck.DiscardPile.Count - 1; i >= 0; i--)
            {
                if (deck.DiscardPile[i].CardName == cardName)
                {
                    deck.DiscardPile.RemoveAt(i);
                    removed = true;
                }
            }
            for (int i = deck.EquippedCards.Count - 1; i >= 0; i--)
            {
                if (deck.EquippedCards[i].CardName == cardName)
                {
                    deck.EquippedCards.RemoveAt(i);
                    removed = true;
                }
            }
            
            if (removed)
            {
                deck.EmitSignal(Deck.SignalName.HandChanged);
                deck.EmitSignal(Deck.SignalName.DeckChanged);
            }
            return removed;
        }

        public static int CountInjuryCards(Deck deck)
        {
            if (deck == null) return 0;
            return deck.DrawPile.Count(c => c.CardClass == CardClass.Injury)
                + deck.Hand.Count(c => c.CardClass == CardClass.Injury)
                + deck.DiscardPile.Count(c => c.CardClass == CardClass.Injury)
                + deck.EquippedCards.Count(c => c.CardClass == CardClass.Injury);
        }
    }
}
