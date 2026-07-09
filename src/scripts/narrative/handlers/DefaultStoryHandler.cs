using DeepForest.Cards;
using DeepForest.Character;

namespace DeepForest.Narrative.Handlers
{
    public class DefaultStoryHandler : ICharacterStoryHandler
    {
        public CharacterId CharacterId => CharacterId.None;
        public CharacterStoryFlags Flags { get; } = new();

        public EndingResult? CheckHiddenStatEndings(Player player, Deck deck) => null;
        public EndingResult? CheckEscapeEndings(Player player, Deck deck) => null;
        public void UnlockBackgroundStories(Player player, Deck deck) { }
        public bool HandleNpcEncounter(Player player, Deck deck, CharacterId npcId, int choice) => false;
        public bool HandleSpecialEvent(Player player, Deck deck, string eventId, int choice) => false;
    }
}
