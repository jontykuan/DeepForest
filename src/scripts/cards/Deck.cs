using Godot;
using System;
using System.Collections.Generic;

namespace DeepForest.Cards;

public partial class Deck : Node
{
    [Signal] public delegate void DeckChangedEventHandler();
    [Signal] public delegate void HandChangedEventHandler();
    [Signal] public delegate void ReshuffledEventHandler();

    public List<Card> DrawPile { get; private set; } = new();
    public List<Card> Hand { get; private set; } = new();
    public List<Card> DiscardPile { get; private set; } = new();
    public List<Card> EquippedCards { get; private set; } = new();

    private Random _random = new();

    public void Initialize(List<Card> startingCards)
    {
        DrawPile.Clear();
        Hand.Clear();
        DiscardPile.Clear();
        EquippedCards.Clear();

        foreach (var card in startingCards)
        {
            DrawPile.Add((Card)card.Duplicate());
        }
        Shuffle(DrawPile);
        EmitSignal(SignalName.DeckChanged);
        EmitSignal(SignalName.HandChanged);
    }

    public void Shuffle(List<Card> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            Card value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public bool DrawCards(int count)
    {
        bool triggeredReshuffle = false;
        for (int i = 0; i < count; i++)
        {
            if (DrawPile.Count == 0)
            {
                if (DiscardPile.Count == 0)
                {
                    break;
                }
                Reshuffle();
                triggeredReshuffle = true;
            }
            Card card = DrawPile[0];
            DrawPile.RemoveAt(0);
            Hand.Add(card);
        }
        EmitSignal(SignalName.HandChanged);
        EmitSignal(SignalName.DeckChanged);
        return triggeredReshuffle;
    }

    public void DiscardHand()
    {
        DiscardPile.AddRange(Hand);
        Hand.Clear();
        EmitSignal(SignalName.HandChanged);
        EmitSignal(SignalName.DeckChanged);
    }

    public void DiscardCard(Card card)
    {
        if (Hand.Remove(card))
        {
            DiscardPile.Add(card);
            EmitSignal(SignalName.HandChanged);
            EmitSignal(SignalName.DeckChanged);
        }
    }

    public void Reshuffle()
    {
        DrawPile.AddRange(DiscardPile);
        DiscardPile.Clear();
        Shuffle(DrawPile);
        EmitSignal(SignalName.Reshuffled);
        EmitSignal(SignalName.DeckChanged);
    }

    public void EquipCard(Card card)
    {
        if (Hand.Remove(card))
        {
            EquippedCards.Add(card);
            
            Card unequipCard = NewUnequipCard(card);
            DiscardPile.Add(unequipCard);
            
            EmitSignal(SignalName.HandChanged);
            EmitSignal(SignalName.DeckChanged);
        }
    }

    public void UnequipCard(Card card, Card unequipCard)
    {
        if (EquippedCards.Remove(card))
        {
            RemoveCardFromAllPiles(unequipCard);
            Hand.Add(card);
            
            EmitSignal(SignalName.HandChanged);
            EmitSignal(SignalName.DeckChanged);
        }
    }

    private Card NewUnequipCard(Card parentCard)
    {
        Card unequip = new Card();
        unequip.CardName = "卸下 " + parentCard.CardName;
        unequip.CardType = CardType.ActionStr; 
        unequip.Weight = parentCard.Weight;    
        unequip.HungerCost = 1;
        unequip.Description = $"卸下裝備 {parentCard.CardName}，將其收回背包。";
        unequip.SetMeta("parent_card", parentCard);
        return unequip;
    }

    private void RemoveCardFromAllPiles(Card card)
    {
        Hand.Remove(card);
        DrawPile.Remove(card);
        DiscardPile.Remove(card);
    }

    public int GetTotalWeight()
    {
        int total = 0;
        foreach (var c in DrawPile) total += c.Weight;
        foreach (var c in Hand) total += c.Weight;
        foreach (var c in DiscardPile) total += c.Weight;
        foreach (var c in EquippedCards) total += c.Weight;
        return total;
    }
}
