using DeepForest.Scene;

namespace DeepForest.Cards.Effects
{
    public class ActionResult
    {
        public bool Success { get; init; }
        public string LogMessage { get; init; } = "";
        public bool RemoveActionAfterUse { get; init; }
        public bool TriggerSceneChange { get; init; }
        public SceneData? NewScene { get; init; }
    }
}
