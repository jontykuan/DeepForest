using Godot;
using System.Text;

namespace DeepForest.Core;

public static class Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }

    public static LogLevel CurrentLogLevel { get; set; } = LogLevel.Debug;

    public static void Debug(string message, params object[] context)
    {
        Log(LogLevel.Debug, message, context);
    }

    public static void Info(string message, params object[] context)
    {
        Log(LogLevel.Info, message, context);
    }

    public static void Warn(string message, params object[] context)
    {
        Log(LogLevel.Warn, message, context);
    }

    public static void Error(string message, params object[] context)
    {
        Log(LogLevel.Error, message, context);
    }

    private static void Log(LogLevel level, string message, object[] context)
    {
        if (level < CurrentLogLevel) return;

        var sb = new StringBuilder();
        sb.Append($"[{level}] {message}");

        if (context != null && context.Length > 0)
        {
            sb.Append(" | Context: { ");
            for (int i = 0; i < context.Length; i += 2)
            {
                if (i > 0) sb.Append(", ");
                string key = context[i]?.ToString() ?? "null";
                string value = (i + 1 < context.Length) ? (context[i + 1]?.ToString() ?? "null") : "null";
                sb.Append($"\"{key}\": \"{value}\"");
            }
            sb.Append(" }");
        }

        string formatted = sb.ToString();

        switch (level)
        {
            case LogLevel.Debug:
            case LogLevel.Info:
                GD.Print(formatted);
                break;
            case LogLevel.Warn:
                GD.PushWarning(formatted);
                break;
            case LogLevel.Error:
                GD.PushError(formatted);
                break;
        }
    }
}
