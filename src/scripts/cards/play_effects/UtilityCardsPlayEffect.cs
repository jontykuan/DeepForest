using System;
using DeepForest.Character;
using DeepForest.Core;
using DeepForest.UI;

namespace DeepForest.Cards.Effects
{
    [CardPlayEffect("空瓶")]
    [CardPlayEffect("重整")]
    [CardPlayEffect("翻找背包")]
    [CardPlayEffect("短暫休息")]
    public class UtilityCardsPlayEffect : ICardPlayEffect
    {
        public bool CanPlay(Card card, Player player, Deck deck, out string message)
        {
            if (card.CardId == CardId.EmptyBottle)
            {
                message = "這是空瓶，你只能在水源處使用『裝水』行動來裝滿它。";
                return false;
            }

            message = "";
            return true;
        }

        public CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message)
        {
            if (card.CardId == CardId.ActionReorganize)
            {
                deck.DrawCards(2);
                message = "你重整了思緒，消耗 5 飢餓與 5 口渴，額外抽取了 2 張牌。";
                return CardPlayHandler.PlayResult.Success;
            }
            else if (card.CardId == CardId.ActionSearchBackpack)
            {
                deck.DiscardCard(card);
                deck.DrawCards(2);
                message = "你翻找了背包，抽了 2 張牌。";
                return CardPlayHandler.PlayResult.Success;
            }
            else if (card.CardId == CardId.ActionRest)
            {
                deck.DiscardCard(card);
                player.CurrentHp = Math.Min(player.MaxHp, player.CurrentHp + 10);
                player.CurrentSanity = Math.Min(player.MaxSanity, player.CurrentSanity + 3);
                message = "你進行了短暫休息（體力 +10，理智 +3）。";
                return CardPlayHandler.PlayResult.Success;
            }

            message = "未知效用的卡牌。";
            return CardPlayHandler.PlayResult.InvalidCard;
        }
    }
}
