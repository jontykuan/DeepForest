using Godot;

namespace DeepForest.Core
{
    [GlobalClass]
    public partial class ColorPalette : Resource
    {
        private static ColorPalette _instance;
        public static ColorPalette Instance => _instance ??= GD.Load<ColorPalette>("res://src/resources/theme/color_palette.tres");

        [Export] public Color RadiantGreen { get; set; } = Color.FromHtml("#39FF14");
        [Export] public Color BloodRed { get; set; } = Color.FromHtml("#FF2222");
        [Export] public Color DarkRed { get; set; } = Color.FromHtml("#660000");
        [Export] public Color DarkBlue { get; set; } = Color.FromHtml("#000066");
        [Export] public Color SilentWhite { get; set; } = Color.FromHtml("#FFFFFF");
        [Export] public Color BrightBlue { get; set; } = Color.FromHtml("#00BFFF");
        [Export] public Color GrayGreen { get; set; } = Color.FromHtml("#556B2F");
        [Export] public Color AmberWarning { get; set; } = Color.FromHtml("#FFB000");
        [Export] public Color PaleGray { get; set; } = Color.FromHtml("#C0C0C0");
        [Export] public Color SoftGray { get; set; } = Color.FromHtml("#808080");
    }
}
