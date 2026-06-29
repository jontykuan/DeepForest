using Godot;
using System;
using System.Collections.Generic;

namespace DeepForest.Narrative;

public partial class StoryUnlock : Node
{
    public static StoryUnlock Instance { get; private set; } = null!;

    private const string SavePath = "user://story_unlocks.cfg";
    private ConfigFile _configFile = new();
    private List<string> _unlockedTitles = new();

    public override void _Ready()
    {
        Instance = this;
        LoadProgress();
    }

    public void UnlockStorySegment(string title, string description)
    {
        if (!_unlockedTitles.Contains(title))
        {
            _unlockedTitles.Add(title);
            _configFile.SetValue("Unlocks", title, description);
            _configFile.Save(SavePath);
            GD.Print($"[StoryUnlock] Unlocked: {title}");
        }
    }

    public void LoadProgress()
    {
        _unlockedTitles.Clear();
        if (_configFile.Load(SavePath) == Error.Ok)
        {
            if (_configFile.HasSection("Unlocks"))
            {
                foreach (string title in _configFile.GetSectionKeys("Unlocks"))
                {
                    _unlockedTitles.Add(title);
                }
            }
        }
    }

    public IReadOnlyList<string> GetUnlockedTitles()
    {
        return _unlockedTitles.AsReadOnly();
    }

    public string GetStoryDescription(string title)
    {
        return _configFile.GetValue("Unlocks", title, "").ToString();
    }
}
