using Godot;

namespace DeepForest.Core
{
    public static class GameConstants
    {
        // Colors
        public static readonly Color RadiantGreen = new Color(0.22f, 1.0f, 0.08f); // #39FF14
        public static readonly Color BloodRed = new Color(1.0f, 0.0f, 0.2f); // #FF0033
        public static readonly Color DarkRed = new Color(0.4f, 0.0f, 0.0f); // #660000
        public static readonly Color DarkBlue = new Color(0.0f, 0.0f, 0.4f); // #000066
        public static readonly Color SilentWhite = new Color(0.9f, 0.9f, 0.9f);
        public static readonly Color SoftGray = new Color(0.5f, 0.5f, 0.5f);

        // Map & Game structure
        public const int MaxDepth = 30;
        public const int WinDay = 20;
    }
}
