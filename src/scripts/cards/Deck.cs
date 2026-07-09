using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Core;
using DeepForest.Character;
using DeepForest.Narrative;

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
            Card cardInstance = (Card)card.Duplicate();
            cardInstance.UsesLeft = cardInstance.MaxUses;
            DrawPile.Add(cardInstance);
        }

        if (GameState.Instance?.PlayerInstance?.CharacterData?.CharacterId == CharacterId.Nancy)
        {
            if (!DrawPile.Exists(c => c.CardId == CardId.KeyRecordingTape))
            {
                Card tape = CardFactory.CreateCard(CardId.KeyRecordingTape);
                if (tape != null) DrawPile.Add(tape);
            }
            if (!DrawPile.Exists(c => c.CardId == CardId.KeyShowdown))
            {
                Card showdown = CardFactory.CreateCard(CardId.KeyShowdown);
                if (showdown != null) DrawPile.Add(showdown);
            }
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

    public bool DrawCards(int count, bool ignoreLimit = false)
    {
        int limit = GameState.Instance.PlayerInstance.HandLimit;
        bool triggeredReshuffle = false;
        for (int i = 0; i < count; i++)
        {
            if (!ignoreLimit && Hand.Count >= limit)
            {
                break;
            }
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
            
            AddCardToHand(card);
        }
        EmitSignal(SignalName.HandChanged);
        EmitSignal(SignalName.DeckChanged);
        return triggeredReshuffle;
    }

    public void AddCardToHand(Card card)
    {
        if (card.CardId == CardId.CurseSuffocation)
        {
            if (CardQueryHelper.HasCardEquipped(this, CardId.KeyJerryCollar))
            {
                SaveManager.UnlockEnding("湯明亮", "成就：好狗狗");
                StoryUnlock.Instance?.UnlockStorySegment("成就：好狗狗", "在被「窒息」折磨的同時，你選擇戴上了傑利的項圈... 成為一隻乖狗。");
            }

            Hand.Add(card);
            GameState.Instance.AddLog("【詛咒】你抽到了【窒息】！感到呼吸極度困難，你被迫丟棄了所有手牌，並重新抽取 1 張牌！");
            foreach (var c in Hand)
            {
                if (!c.HasMeta("temporary"))
                {
                    DiscardPile.Add(c);
                }
            }
            Hand.Clear();
            DrawCards(1);
        }
        else
        {
            Hand.Add(card);
            if (card.EffectTags.HasFlag(CardEffectTag.Corruption))
            {
                GameState.Instance.PlayerInstance.CurrentSanity -= 15;
                GameState.Instance.AddLog("【詛咒】你抽到了【穢祟附身】！陰冷的低語侵蝕了你的理智（理智 -15）！");
            }
        }
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
            if (!card.HasMeta("temporary"))
            {
                DiscardPile.Add(card);
            }
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

            if (card.CardId == CardId.KeyJerryCollar && CardQueryHelper.HasCardAnywhere(this, CardId.CurseSuffocation))
            {
                SaveManager.UnlockEnding("湯明亮", "成就：好狗狗");
                StoryUnlock.Instance?.UnlockStorySegment("成就：好狗狗", "在被「窒息」折磨的同時，你選擇戴上了傑利的項圈... 成為一隻乖狗。");
            }
            if (card.CardId != CardId.KeyJerryCollar)
            {
                Card unequipCard = NewUnequipCard(card);
                DiscardPile.Add(unequipCard);
            }
            
            EmitSignal(SignalName.HandChanged);
            EmitSignal(SignalName.DeckChanged);
        }
    }

    public void UnequipCard(Card card, Card unequipCard)
    {
        if (EquippedCards.Remove(card))
        {
            RemoveCardFromAllPiles(unequipCard);
            AddCardToHand(card);
            
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
        return total;
    }

    public bool AddCardToDiscardPile(Card card)
    {
        DiscardPile.Add(card);
        EmitSignal(SignalName.DeckChanged);
        return true;
    }
}
