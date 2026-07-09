using System;
using System.Collections.Generic;
using System.Linq;
using DeepForest.Character;
using DeepForest.Core;
using DeepForest.UI;

namespace DeepForest.Cards.Effects
{
    [CardPlayEffect("吹牛")]
    public class BoastPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            int val = new Random().Next(-1, 4); // -1 to 3
            TurnManager.Instance.AccumulatedStr += val;
            TurnManager.Instance.AccumulatedDex += val;
            TurnManager.Instance.AccumulatedWis += val;

            deck.DiscardCard(card);
            message = $"你大聲吹牛，隨機獲得了 {val} 點不限類別的屬性點數！";
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("規劃")]
    public class PlanPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            deck.DrawCards(3);

            deck.Hand.Remove(card);
            deck.DiscardPile.Add(card);
            deck.EmitSignal(Deck.SignalName.HandChanged);
            deck.EmitSignal(Deck.SignalName.DeckChanged);

            GameState.Instance.CurrentInteractionState = GameState.InteractionState.PlanningDiscard;
            message = "你打出了【規劃】：額外抽取了 3 張牌。現在請選擇 1 張手牌進行【棄置】。";
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("自殘")]
    public class SelfHarmPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            int hpLost = Math.Max(5, player.CurrentHp / 10);
            player.CurrentHp -= hpLost;
            player.CurrentSanity = Math.Min(player.MaxSanity, player.CurrentSanity + hpLost);

            var cutCard = CardFactory.CreateCard(CardId.InjuryCut);
            if (cutCard != null)
            {
                deck.AddCardToDiscardPile(cutCard);
            }

            deck.DiscardCard(card);
            message = $"你進行了【自殘】：消耗了 {hpLost} 點體力，回復了 {hpLost} 點理智，並將 1 張傷勢卡【割痕】放入棄牌堆。";
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("新生")]
    public class RebirthPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            List<Card> negativeCards = new List<Card>();
            foreach (var c in deck.Hand)
            {
                if (c.CardType == CardType.Curse || c.CardType == CardType.Injury || c.CardId == CardId.InjuryFatigue)
                {
                    negativeCards.Add(c);
                }
            }

            int count = negativeCards.Count;
            if (count > 0)
            {
                foreach (var c in negativeCards)
                {
                    deck.Hand.Remove(c);
                    deck.DiscardPile.Add(c);
                }
                player.CurrentSanity -= (count - 1);
                int hpGain = (int)Math.Ceiling(count / 2.0);
                player.CurrentHp = Math.Min(player.MaxHp, player.CurrentHp + hpGain);
                message = $"你獲得了【新生】：拋棄手牌中 {count} 張負面卡，額外扣減了 {count - 1} 點理智，共消耗 {count} 點理智，但恢復了 {hpGain} 點體力。";
            }
            else
            {
                message = "手牌中沒有負面卡，新生效果未觸發任何拋棄。";
            }

            deck.DiscardCard(card);
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("吹口哨")]
    public class WhistlePlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            Card? dog = null;
            // Check draw pile
            foreach (var c in deck.DrawPile)
            {
                if (c.CardId == CardId.KeyJerry || c.CardId == CardId.KeyJerryQuestion)
                {
                    dog = c;
                    deck.DrawPile.Remove(c);
                    break;
                }
            }
            if (dog == null)
            {
                // Check discard pile
                foreach (var c in deck.DiscardPile)
                {
                    if (c.CardId == CardId.KeyJerry || c.CardId == CardId.KeyJerryQuestion)
                    {
                        dog = c;
                        deck.DiscardPile.Remove(c);
                        break;
                    }
                }
            }

            if (dog != null)
            {
                deck.Hand.Add(dog);
                message = $"你吹響了口哨，呼喚小狗的聲音傳開，你將【{dog.CardName}】拿到了手中！";
            }
            else
            {
                int r = new Random().Next(3);
                if (r == 0)
                {
                    TurnManager.Instance.AccumulatedStr += 1;
                    message = "你吹響了口哨，但周圍只有林風迴響。你在警覺中獲得了力量 1。";
                }
                else if (r == 1)
                {
                    TurnManager.Instance.AccumulatedDex += 1;
                    message = "你吹響了口哨，但周圍只有林風迴響。你在警覺中獲得了靈巧 1。";
                }
                else
                {
                    TurnManager.Instance.AccumulatedWis += 1;
                    message = "你吹響了口哨，但周圍只有林風迴響。你在警覺中獲得了智慧 1。";
                }
            }

            deck.DiscardCard(card);
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("捉迷藏")]
    public class HideAndSeekPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            List<Card> drawn = new List<Card>();
            for (int i = 0; i < 2; i++)
            {
                int beforeCount = deck.Hand.Count;
                deck.DrawCards(1);
                if (deck.Hand.Count > beforeCount)
                {
                    drawn.Add(deck.Hand[deck.Hand.Count - 1]);
                }
            }

            int dexGain = 0;
            int strGain = 0;
            int fatigueCount = 0;

            foreach (var c in drawn)
            {
                if (c.CardType == CardType.ActionStr || c.CardType == CardType.ActionDex || c.CardType == CardType.ActionWis)
                {
                    dexGain++;
                }
                else if (c.CardType == CardType.Consumable || c.CardType == CardType.Equipment || c.CardType == CardType.KeyItem)
                {
                    strGain++;
                }
                else if (c.CardType == CardType.Curse || c.CardType == CardType.Injury || c.CardId == CardId.InjuryFatigue)
                {
                    fatigueCount++;
                    var fatigue = CardFactory.CreateCard(CardId.InjuryFatigue);
                    if (fatigue != null)
                    {
                        deck.AddCardToDiscardPile(fatigue);
                    }
                }
            }

            TurnManager.Instance.AccumulatedDex += dexGain;
            TurnManager.Instance.AccumulatedStr += strGain;

            deck.DiscardCard(card);
            message = $"你進行了【捉迷藏】並抽了 {drawn.Count} 張牌：獲得了 靈巧 +{dexGain}，力量 +{strGain}；額外增加了 {fatigueCount} 張【疲勞】。";
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("躲貓貓")]
    public class PeekABooPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            Card? toDiscard = null;
            foreach (var c in deck.Hand)
            {
                if (c != card)
                {
                    toDiscard = c;
                    break;
                }
            }

            if (toDiscard != null)
            {
                deck.Hand.Remove(toDiscard);
                deck.DiscardPile.Add(toDiscard);

                List<Card> drawn = new List<Card>();
                for (int i = 0; i < 2; i++)
                {
                    int beforeCount = deck.Hand.Count;
                    deck.DrawCards(1);
                    if (deck.Hand.Count > beforeCount)
                    {
                        drawn.Add(deck.Hand[deck.Hand.Count - 1]);
                    }
                }

                bool match = false;
                foreach (var c in drawn)
                {
                    if (c.CardType == toDiscard.CardType)
                    {
                        match = true;
                        break;
                    }
                }

                if (match)
                {
                    TurnManager.Instance.AccumulatedWis += 5;
                    message = $"你進行了【躲貓貓】：丟棄了【{toDiscard.CardName}】並抽了 {drawn.Count} 張牌。成功匹配卡牌類型！獲得智慧 +5！";
                }
                else
                {
                    message = $"你進行了【躲貓貓】：丟棄了【{toDiscard.CardName}】並抽了 {drawn.Count} 張牌。類型未匹配，未能獲得智慧。";
                }
            }
            else
            {
                message = "手牌中沒有其他卡牌可供丟棄！";
            }

            deck.DiscardCard(card);
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("花名冊")]
    public class RosterPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            deck.Hand.Remove(card);
            deck.DrawCards(5);
            message = "你翻閱了【花名冊】，上面的名字與關聯關係觸目驚心...（飢餓 -8，已抽取了 5 張卡牌）。";
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("攤牌")]
    public class ShowdownPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            deck.Hand.Remove(card);

            List<Card> candidates = new List<Card>();
            int count = Math.Min(3, deck.DrawPile.Count);
            if (count == 0 && deck.DiscardPile.Count > 0)
            {
                deck.Reshuffle();
                count = Math.Min(3, deck.DrawPile.Count);
            }

            for (int i = 0; i < count; i++)
            {
                candidates.Add(deck.DrawPile[0]);
                deck.DrawPile.RemoveAt(0);
            }

            if (candidates.Count > 0)
            {
                // Select best card: KeyItem > Action > Consumable > Curse/Injury
                Card best = candidates.OrderBy(c => c.CardType switch
                {
                    CardType.KeyItem => 0,
                    CardType.ActionWis => 1,
                    CardType.ActionDex => 1,
                    CardType.ActionStr => 1,
                    CardType.Consumable => 2,
                    _ => 3
                }).First();

                deck.Hand.Add(best);
                candidates.Remove(best);

                foreach (var rest in candidates)
                {
                    deck.DiscardPile.Add(rest);
                }

                string discardedStr = candidates.Count > 0 
                    ? string.Join("、", candidates.Select(c => $"【{c.CardName}】")) 
                    : "無";

                message = $"你進行了【攤牌】：口渴 -2。檢視了牌庫頂的卡牌，挑選了【{best.CardName}】加入手牌；其餘卡牌（{discardedStr}）進入了棄牌堆。";
            }
            else
            {
                message = "你進行了【攤牌】：口渴 -2。但此時牌庫與棄牌堆皆已空無一物。";
            }

            deck.DiscardCard(card);
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("心機")]
    public class SchemePlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            deck.Hand.Remove(card);
            deck.DrawCards(2);

            List<Card> discarded = new List<Card>();
            for (int i = 0; i < 2; i++)
            {
                if (deck.Hand.Count == 0) break;

                // Priority for discarding: Curse > Injury > Consumable > Actions
                Card worst = deck.Hand.OrderBy(c => c.CardType switch
                {
                    CardType.Curse => 0,
                    CardType.Injury => 1,
                    CardType.Consumable => 2,
                    _ => 3
                }).First();

                deck.Hand.Remove(worst);
                
                if (!worst.HasMeta("temporary"))
                {
                    deck.DiscardPile.Add(worst);
                }
                discarded.Add(worst);
            }

            string discardedStr = discarded.Count > 0 
                ? string.Join("、", discarded.Select(c => $"【{c.CardName}】")) 
                : "無";

            message = $"你展現了【心機】：口渴 -3。抽取了 2 張卡牌，並棄置了 2 張手牌（{discardedStr}）。";

            if (card.HasMeta("temporary"))
            {
                deck.EmitSignal(Deck.SignalName.HandChanged);
                deck.EmitSignal(Deck.SignalName.DeckChanged);
            }
            else
            {
                deck.DiscardCard(card);
            }

            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("耳語")]
    public class WhisperPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            deck.Hand.Remove(card);

            Card schemeRes = CardFactory.CreateCard(CardId.KeyScheme);
            if (schemeRes != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    Card tempScheme = (Card)schemeRes.Duplicate();
                    tempScheme.SetMeta("temporary", true);
                    deck.Hand.Add(tempScheme);
                }
                message = "古老的邪靈在你耳邊呢喃... 獲得了 2 張臨時的【心機】！";
            }
            else
            {
                message = "古老的邪靈在你耳邊呢喃，但你甚麼也沒聽清。";
            }

            deck.DiscardCard(card);
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("反抗")]
    public class RebellionPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            player.CurrentThirst -= 5;
            
            if (deck.DiscardPile.Count > 0)
            {
                Card recovered = deck.DiscardPile[deck.DiscardPile.Count - 1];
                deck.DiscardPile.RemoveAt(deck.DiscardPile.Count - 1);
                deck.AddCardToHand(recovered);
                message = $"【反抗】口渴 -5。從棄牌堆中回收了【{recovered.CardName}】放入手牌！";
            }
            else
            {
                message = "【反抗】口渴 -5。但棄牌堆中沒有任何卡牌可回收。";
            }

            deck.DiscardCard(card);
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("複製")]
    public class CopyPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            player.Brutality = Math.Min(100, player.Brutality + 3);
            GameState.Instance.CopyNextCard = true;
            message = "【複製】暴戾 +3。下一張打出的卡牌將再次觸發其效果！";
            deck.DiscardCard(card);
            return CardPlayHandler.PlayResult.Success;
        }
    }

    [CardPlayEffect("狂亂")]
    public class MadnessPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            deck.Hand.Remove(card);
            player.CurrentSanity = Math.Max(0, player.CurrentSanity - 20);
            GameState.Instance.IsDescentActive = true;
            message = "【狂亂之念】你打出了『狂亂』！心智崩潰，理智 -20，降神儀式在四周開啟！";
            deck.DiscardCard(card);
            return CardPlayHandler.PlayResult.Success;
        }
    }
}
