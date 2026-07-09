using DeepForest.Cards;
using DeepForest.Character;

namespace DeepForest.Narrative
{
    public record EndingResult(string Type, string Title, string Description);

    public interface ICharacterStoryHandler
    {
        CharacterId CharacterId { get; }
        CharacterStoryFlags Flags { get; }

        EndingResult? CheckHiddenStatEndings(Player player, Deck deck);
        EndingResult? CheckEscapeEndings(Player player, Deck deck);
        void UnlockBackgroundStories(Player player, Deck deck);
        bool HandleNpcEncounter(Player player, Deck deck, CharacterId npcId, int choice);
        bool HandleSpecialEvent(Player player, Deck deck, string eventId, int choice);
    }
}
