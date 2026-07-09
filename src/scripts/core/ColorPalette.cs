using Godot;

namespace DeepForest.Core
{
    [GlobalClass]
    public partial class ColorPalette : Resource
    {
        [Export] public Color RadiantGreen { get; set; } = new Color(0.22f, 1.0f, 0.08f);
        [Export] public Color BloodRed { get; set; } = new Color(1.0f, 0.0f, 0.2f);
        [Export] public Color DarkRed { get; set; } = new Color(0.4f, 0.0f, 0.0f);
        [Export] public Color DarkBlue { get; set; } = new Color(0.0f, 0.0f, 0.4f);
        [Export] public Color SilentWhite { get; set; } = new Color(0.9f, 0.9f, 0.9f);
        [Export] public Color SoftGray { get; set; } = new Color(0.5f, 0.5f, 0.5f);
    }
}
