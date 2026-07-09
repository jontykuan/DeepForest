using Godot;

namespace DeepForest.Scene;

[GlobalClass]
public partial class SceneData : Resource
{
    [Export] public string SceneName { get; set; } = "Unnamed Scene";
    [Export(PropertyHint.MultilineText)] public string SceneDescription { get; set; } = "";
    [Export] public Godot.Collections.Array<SceneAction> Actions { get; set; } = new();

    // 4 區塊動態合成參數
    [Export] public string BottomGround { get; set; } = "dirt";
    [Export] public string LeftTerrain { get; set; } = "woodland";
    [Export] public string RightTerrain { get; set; } = "woodland";
    [Export] public Godot.Collections.Array<string> Decals { get; set; } = new();
}
