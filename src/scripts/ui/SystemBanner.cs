using Godot;
using System;
using DeepForest.Core;

namespace DeepForest.UI
{
    public class SystemBanner
    {
        private readonly Label _systemBannerLabel;

        public SystemBanner(Label systemBannerLabel)
        {
            _systemBannerLabel = systemBannerLabel;
        }

        public void UpdateSystemBanner(string customLog = "")
        {
            if (_systemBannerLabel == null) return;

            var env = EnvironmentSystem.Instance;
            string bannerText = $"第 {GameState.Instance.CurrentDay} 天 │ 森林深度: {GameState.Instance.CurrentDepth}";
            if (env != null)
            {
                float tempF = env.CurrentTempCelsius * 9f / 5f + 32f;
                bannerText += $"|天氣：{env.GetWeatherString()}|溫度：{env.CurrentTempCelsius:F1}°C/{tempF:F1}°F|濕度：{env.CurrentHumidityPercent:F0}%";
            }

            if (!string.IsNullOrEmpty(customLog))
            {
                bannerText += $"\n> {customLog}";
            }
            else if (GameState.Instance.GameLogs.Count > 0)
            {
                bannerText += $"\n> {GameState.Instance.GameLogs[GameState.Instance.GameLogs.Count - 1]}";
            }
            
            _systemBannerLabel.Text = bannerText;
        }
    }
}
