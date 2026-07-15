using Godot;
using System;
using DeepForest.Core;
using ColorPalette = DeepForest.Core.ColorPalette;

namespace DeepForest.UI
{
    public class SystemBanner
    {
        private readonly RichTextLabel _systemBannerLabel;

        public SystemBanner(RichTextLabel systemBannerLabel)
        {
            _systemBannerLabel = systemBannerLabel;
        }

        public void UpdateSystemBanner(string customLog = "")
        {
            if (_systemBannerLabel == null) return;

            var cp = ColorPalette.Instance;
            var env = EnvironmentSystem.Instance;
            
            string whiteHex = cp.SilentWhite.ToHtml(false);
            string greenHex = cp.RadiantGreen.ToHtml(false);
            string blueHex = cp.BrightBlue.ToHtml(false);
            string amberHex = cp.AmberWarning.ToHtml(false);
            string grayHex = cp.PaleGray.ToHtml(false);

            string text = $"第 {GameState.Instance.CurrentDay} 天 │ 森林深度: {GameState.Instance.CurrentDepth}M";
            if (env != null)
            {
                float tempF = env.CurrentTempCelsius * 9f / 5f + 32f;
                string weatherString = env.GetWeatherString();
                text += $" │ 天氣：{weatherString} │ 溫度：{env.CurrentTempCelsius:F1}°C/{tempF:F1}°F │ 濕度：{env.CurrentHumidityPercent:F0}%";
            }
            string bannerText = $"[color=#{greenHex}]{text}[/color]";

            string logEntry = "";
            if (!string.IsNullOrEmpty(customLog))
            {
                logEntry = customLog;
            }
            else if (GameState.Instance.GameLogs.Count > 0)
            {
                logEntry = GameState.Instance.GameLogs[GameState.Instance.GameLogs.Count - 1];
            }

            string resultText = $"[center]{bannerText}[/center]";
            if (!string.IsNullOrEmpty(logEntry))
            {
                resultText += $"\n[center][color=#{grayHex}]>[/color] [color=#{grayHex}]{logEntry}[/color][/center]";
            }
            
            _systemBannerLabel.Text = resultText;
        }
    }
}
