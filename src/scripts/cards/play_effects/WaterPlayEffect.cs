using System;
using DeepForest.Character;
using DeepForest.Core;
using DeepForest.UI;

namespace DeepForest.Cards.Effects
{
    [CardPlayEffect("礦泉水")]
    [CardPlayEffect("生水")]
    public class WaterPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            if (card.CardId == CardId.ConsumableWater)
            {
                deck.Hand.Remove(card); 
                var emptyBottle = CardFactory.CreateConsumableCard(CardId.EmptyBottle, "空瓶", 0, 0, 0, 0);
                emptyBottle.Weight = 2;
                emptyBottle.Description = "空瓶：重量2，在牌組中可以在水源處使用裝水。";
                deck.AddCardToDiscardPile(emptyBottle);
                message = "飲用了【礦泉水】（回復口渴 15）。剩下了一個【空瓶】放回背包。";
                return CardPlayHandler.PlayResult.Success;
            }
            else // 生水
            {
                deck.Hand.Remove(card); 
                var emptyBottle = CardFactory.CreateConsumableCard(CardId.EmptyBottle, "空瓶", 0, 0, 0, 0);
                emptyBottle.Weight = 2;
                emptyBottle.Description = "空瓶：重量2，在牌組中可以在水源處使用裝水。";
                deck.AddCardToDiscardPile(emptyBottle);

                string illnessMsg = "";
                if (Random.Shared.NextSingle() < 0.3f)
                {
                    var diarrhea = CardFactory.CreateInjuryCard(CardId.InjuryGastroenteritis, "腸胃炎", "生病：因為生飲生水導致急性腸胃炎，腹部劇烈絞痛。");
                    diarrhea.EffectTags = CardEffectTag.WoundInfection;
                    deck.AddCardToDiscardPile(diarrhea);
                    illnessMsg = " 突然間你感到一陣嚴重的腹部絞痛，你生病了！（獲得【腸胃炎】傷勢卡）";
                }
                message = $"飲用了【生水】（回復飢渴 10）。剩下了一個【空瓶】。{illnessMsg}";
                return CardPlayHandler.PlayResult.Success;
            }
        }
    }
}
