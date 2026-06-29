using Godot;
using System.Collections.Generic;
using DeepForest.Character;
using DeepForest.Cards;
using DeepForest.Combat;
using DeepForest.Narrative;

namespace DeepForest.Core;

public partial class GameState : Node
{
    public static GameState Instance { get; private set; } = null!;

    [Signal] public delegate void LogAddedEventHandler(string logMessage);
    [Signal] public delegate void DayChangedEventHandler(int newDay);
    [Signal] public delegate void DepthChangedEventHandler(int newDepth);

    public Player PlayerInstance { get; set; } = new();
    public Deck DeckInstance { get; set; } = new();
    public CombatManager CombatManagerInstance { get; set; } = new();
    public EndingManager EndingManagerInstance { get; set; } = new();
    public StoryUnlock StoryUnlockInstance { get; set; } = new();

    public bool IsIndoor { get; set; } = false;
    public int IndoorDepth { get; set; } = 0;
    public int EntranceNodeId { get; set; } = 0;

    private int _currentDay = 1;
    private int _currentDepth = 0;
    private List<string> _gameLogs = new();

    public int CurrentDay
    {
        get => _currentDay;
        set 
        { 
            _currentDay = value; 
            EmitSignal(SignalName.DayChanged, _currentDay); 
        }
    }

    public int CurrentDepth
    {
        get => _currentDepth;
        set 
        { 
            _currentDepth = value; 
            EmitSignal(SignalName.DepthChanged, _currentDepth); 
        }
    }

    public IReadOnlyList<string> GameLogs => _gameLogs.AsReadOnly();

    public override void _EnterTree()
    {
        Instance = this;
        AddChild(PlayerInstance);
        AddChild(DeckInstance);
        AddChild(CombatManagerInstance);
        AddChild(EndingManagerInstance);
        AddChild(StoryUnlockInstance);
    }


    public void AddLog(string message)
    {
        _gameLogs.Add(message);
        EmitSignal(SignalName.LogAdded, message);
        GD.Print(message);
    }

    public void ClearLogs()
    {
        _gameLogs.Clear();
    }
}
