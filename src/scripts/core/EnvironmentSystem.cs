using Godot;
using System;

namespace DeepForest.Core;

public enum WeatherType { Clear, Cloudy, Foggy, Rainy, Thunderstorm }
public enum TempType { Freezing, Cool, Warm, Hot, Scorching }
public enum HumidityType { Dry, Moderate, Humid, ExtremelyHumid }

public partial class EnvironmentSystem : Node
{
    public static EnvironmentSystem Instance { get; private set; } = null!;

    public WeatherType Weather { get; set; } = WeatherType.Clear;
    public TempType Temperature { get; set; } = TempType.Cool;
    public HumidityType Humidity { get; set; } = HumidityType.Moderate;

    public float CurrentTempCelsius { get; private set; } = 15f;
    public float CurrentHumidityPercent { get; private set; } = 60f;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        GenerateRandomEnvironment();
    }

    private readonly Random _random = new Random();

    private double NextGaussian(double mean, double stdDev)
    {
        double u1 = 1.0 - _random.NextDouble(); 
        double u2 = 1.0 - _random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }

    public void GenerateRandomEnvironment()
    {
        // 溫帶森林氣候的常態分佈
        // 天氣: 0=晴朗, 1=陰天, 2=濃霧, 3=暴雨, 4=雷暴。中心設在陰天(1.0)，標準差1.0
        int w = (int)Math.Round(NextGaussian(1.0, 1.0));
        Weather = (WeatherType)Math.Clamp(w, 0, 4);

        // 溫度: 溫帶森林氣候的常態分佈 (攝氏)，平均 15度，標準差 8度
        CurrentTempCelsius = (float)NextGaussian(15.0, 8.0);
        if (CurrentTempCelsius <= 0) Temperature = TempType.Freezing;
        else if (CurrentTempCelsius <= 15) Temperature = TempType.Cool;
        else if (CurrentTempCelsius <= 25) Temperature = TempType.Warm;
        else if (CurrentTempCelsius <= 35) Temperature = TempType.Hot;
        else Temperature = TempType.Scorching;

        // 濕度: 平均 65%，標準差 15%
        CurrentHumidityPercent = Math.Clamp((float)NextGaussian(65.0, 15.0), 0f, 100f);
        if (CurrentHumidityPercent < 30) Humidity = HumidityType.Dry;
        else if (CurrentHumidityPercent < 60) Humidity = HumidityType.Moderate;
        else if (CurrentHumidityPercent < 85) Humidity = HumidityType.Humid;
        else Humidity = HumidityType.ExtremelyHumid;
    }

    public string GetWeatherString() => Weather switch
    {
        WeatherType.Clear => "晴朗",
        WeatherType.Cloudy => "陰天",
        WeatherType.Foggy => "濃霧",
        WeatherType.Rainy => "暴雨",
        WeatherType.Thunderstorm => "雷暴",
        _ => "未知"
    };

    public string GetTempString() => Temperature switch
    {
        TempType.Freezing => "寒冷",
        TempType.Cool => "涼爽",
        TempType.Warm => "溫暖",
        TempType.Hot => "炎熱",
        TempType.Scorching => "酷熱",
        _ => "未知"
    };

    public string GetHumidityString() => Humidity switch
    {
        HumidityType.Dry => "乾燥",
        HumidityType.Moderate => "適中",
        HumidityType.Humid => "潮濕",
        HumidityType.ExtremelyHumid => "極濕",
        _ => "未知"
    };

    public int GetDexThresholdModifier()
    {
        if (Weather == WeatherType.Foggy || Weather == WeatherType.Rainy || Weather == WeatherType.Thunderstorm)
            return 2;
        return 0;
    }

    public int GetThirstCostModifier()
    {
        if (Temperature == TempType.Hot || Temperature == TempType.Scorching)
            return 1;
        return 0;
    }

    public float GetRestRecoveryMultiplier()
    {
        float mult = 1.0f;
        if (Humidity == HumidityType.Humid || Humidity == HumidityType.ExtremelyHumid)
            mult -= 0.3f;
        if (Weather == WeatherType.Rainy || Weather == WeatherType.Thunderstorm)
            mult -= 0.2f;
        return Math.Max(0.2f, mult);
    }
}
