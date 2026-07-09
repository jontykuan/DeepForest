using DeepForest.Core;
using DeepForest.Cards;
using DeepForest.Scene;
using DeepForest.Character;

namespace DeepForest.Cards.Effects
{
    public class ActionContext
    {
        public GameState GameState { get; init; } = null!;
        public Player Player { get; init; } = null!;
        public Deck Deck { get; init; } = null!;
        public MapManager MapManager { get; init; } = null!;
        public TurnManager TurnManager { get; init; } = null!;
        public EnvironmentSystem Environment { get; init; } = null!;
        public SceneAction SourceAction { get; init; } = null!;
        public SceneData CurrentScene { get; init; } = null!;
    }
}
