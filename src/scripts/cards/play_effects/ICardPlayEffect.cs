using DeepForest.Character;
using DeepForest.Core;
using DeepForest.UI;

namespace DeepForest.Cards.Effects
{
    public interface ICardPlayEffect
    {
        bool CanPlay(Card card, Player player, Deck deck, out string message);
        CardPlayHandler.PlayResult Execute(Card card, Player player, Deck deck, out string message);
    }
}
