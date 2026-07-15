using Godot;
using System.Collections.Generic;
using DeepForest.Character;
using DeepForest.Cards;
using DeepForest.Combat;
using DeepForest.Narrative;

namespace DeepForest.Core;

public partial class GameState : Node
{
    public enum InteractionState
    {
        Normal,
        PlanningDiscard,
        PlanningPutBack
    }

    public InteractionState CurrentInteractionState { get; set; } = InteractionState.Normal;

    public static GameState Instance { get; set; } = null!;
    public bool StimulantActive { get; set; } = false;
    public bool CopyNextCard { get; set; } = false;

    public bool IsDescentActive { get; set; } = false;
    public int SarahCampRemovals { get; set; } = 0;
    public bool NancySuicideFlag { get; set; } = false;
    public bool CelinStalkingActive { get; set; } = false;
    public int CelinStalkDay { get; set; } = 0;

    [Signal] public delegate void LogAddedEventHandler(string logMessage);
    [Signal] public delegate void DayChangedEventHandler(int newDay);
    [Signal] public delegate void DepthChangedEventHandler(int newDepth);

    public Player PlayerInstance { get; set; } = new();
    public Deck DeckInstance { get; set; } = new();
    public EndingManager EndingManagerInstance { get; set; } = new();
    public StoryUnlock StoryUnlockInstance { get; set; } = new();
    public ActionLogger Logger { get; set; } = new();
    private ICharacterStoryHandler _currentStoryHandler = new DeepForest.Narrative.Handlers.DefaultStoryHandler();
    public ICharacterStoryHandler CurrentStoryHandler
    {
        get
        {
            if (PlayerInstance != null && PlayerInstance.CharacterData != null)
            {
                var charId = PlayerInstance.CharacterData.CharacterId;
                if (_currentStoryHandler == null || _currentStoryHandler.CharacterId != charId)
                {
                    _currentStoryHandler = DeepForest.Narrative.Handlers.StoryHandlerFactory.Create(charId);
                }
            }
            return _currentStoryHandler;
        }
        set => _currentStoryHandler = value;
    }

    public bool IsIndoor { get; set; } = false;
    public int IndoorDepth { get; set; } = 0;
    public int EntranceNodeId { get; set; } = 0;

    public bool IsInCombat 
    { 
        get => CombatManager.Instance != null && CombatManager.Instance.IsInCombat;
        set { if (CombatManager.Instance != null) CombatManager.Instance.IsInCombat = value; }
    }
    public EnemyData? CurrentEnemy
    {
        get => CombatManager.Instance?.CurrentEnemy;
        set { if (CombatManager.Instance != null) CombatManager.Instance.CurrentEnemy = value; }
    }
    public int CurrentEnemyHp
    {
        get => CombatManager.Instance != null ? CombatManager.Instance.CurrentEnemyHp : 0;
        set { if (CombatManager.Instance != null) CombatManager.Instance.CurrentEnemyHp = value; }
    }
    public List<Card> CombatPlayedCards => CombatManager.Instance != null ? CombatManager.Instance.CombatPlayedCards : new List<Card>();

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
            _currentDepth = Math.Max(0, value); 
            EmitSignal(SignalName.DepthChanged, _currentDepth); 
        }
    }

    public IReadOnlyList<string> GameLogs => _gameLogs.AsReadOnly();

    public override void _EnterTree()
    {
        Instance = this;
        AddChild(PlayerInstance);
        AddChild(DeckInstance);
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
