using Godot;

namespace DeepForest.Scene;

[GlobalClass]
public partial class SceneData : Resource
{
    [Export] public string SceneName { get; set; } = "Unnamed Scene";
    [Export] public string SceneDescription { get; set; } = "";
    [Export] public Godot.Collections.Array<SceneAction> Actions { get; set; } = new();
    [Export] public string BackgroundAsciiArtFile { get; set; } = ""; 
}
