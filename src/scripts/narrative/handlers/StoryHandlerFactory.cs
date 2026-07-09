using DeepForest.Character;

namespace DeepForest.Narrative.Handlers
{
    public static class StoryHandlerFactory
    {
        public static ICharacterStoryHandler Create(CharacterId characterId)
        {
            return characterId switch
            {
                CharacterId.John => new JohnStoryHandler(),
                CharacterId.Sarah => new SarahStoryHandler(),
                CharacterId.Nancy => new NancyStoryHandler(),
                CharacterId.Leo => new LeoStoryHandler(),
                CharacterId.Celin => new CelinStoryHandler(),
                CharacterId.Tommy => new TommyStoryHandler(),
                _ => new DefaultStoryHandler()
            };
        }
    }
}
