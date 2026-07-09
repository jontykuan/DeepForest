using DeepForest.Character;
using DeepForest.Core;
using DeepForest.UI;

namespace DeepForest.Cards.Effects
{
    [CardPlayEffect("營養口糧(3/3)")]
    [CardPlayEffect("營養口糧(2/3)")]
    [CardPlayEffect("營養口糧(1/3)")]
    public class RationsPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            if (card.CardId == CardId.ConsumableRations3)
            {
                deck.Hand.Remove(card); 
                var nextRation = CardFactory.CreateConsumableCard(CardId.ConsumableRations2, "營養口糧(2/3)", 0, 0, -5, 3);
                nextRation.Weight = 3;
                nextRation.Description = "使用 營養口糧(2/3)。回復飢餓 5，口渴 -3。使用後，加入一張「營養口糧(1/3)」到牌組中並消滅本卡。";
                deck.AddCardToDiscardPile(nextRation);
                message = "使用了【營養口糧(3/3)】（回復飢餓 5，口渴 -3）。剩餘 2 次使用次數，【營養口糧(2/3)】已放入背包。";
                return CardPlayHandler.PlayResult.Success;
            }
            else if (card.CardId == CardId.ConsumableRations2)
            {
                deck.Hand.Remove(card); 
                var nextRation = CardFactory.CreateConsumableCard(CardId.ConsumableRations1, "營養口糧(1/3)", 0, 0, -5, 3);
                nextRation.Weight = 3;
                nextRation.Description = "使用 營養口糧(1/3)。回復飢餓 5，口渴 -3。使用後，將會徹底消耗完畢。";
                deck.AddCardToDiscardPile(nextRation);
                message = "使用了【營養口糧(2/3)】（回復飢餓 5，口渴 -3）。剩餘 1 次使用次數，【營養口糧(1/3)】已放入背包。";
                return CardPlayHandler.PlayResult.Success;
            }
            else // 營養口糧(1/3)
            {
                deck.Hand.Remove(card); 
                message = "使用了【營養口糧(1/3)】（回復飢餓 5，口渴 -3）。口糧已全部消耗完畢！";
                return CardPlayHandler.PlayResult.Success;
            }
        }
    }
}
