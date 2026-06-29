using Godot;
using System;

namespace DeepForest.Character;

public partial class Player : Node
{
    [Signal] public delegate void StatChangedEventHandler(string statName, int oldValue, int newValue);

    private int _maxHp = 100;
    private int _currentHp = 100;
    
    private int _maxSanity = 100;
    private int _currentSanity = 100;
    
    private int _maxHunger = 50;
    private int _currentHunger = 50;
    
    private int _maxThirst = 50;
    private int _currentThirst = 50;

    private int _brutality = 0;
    private int _corruption = 0;
    private int _evil = 0;

    public int MaxHp 
    { 
        get => _maxHp; 
        set { int old = _maxHp; _maxHp = value; EmitSignal(SignalName.StatChanged, "MaxHp", old, value); }
    }
    
    public int CurrentHp
    {
        get => _currentHp;
        set 
        { 
            int old = _currentHp; 
            _currentHp = Math.Clamp(value, 0, _maxHp); 
            EmitSignal(SignalName.StatChanged, "CurrentHp", old, _currentHp); 
        }
    }
    
    public int MaxSanity
    {
        get => _maxSanity;
        set { int old = _maxSanity; _maxSanity = value; EmitSignal(SignalName.StatChanged, "MaxSanity", old, value); }
    }
    
    public int CurrentSanity
    {
        get => _currentSanity;
        set 
        { 
            int old = _currentSanity; 
            _currentSanity = Math.Clamp(value, 0, _maxSanity); 
            EmitSignal(SignalName.StatChanged, "CurrentSanity", old, _currentSanity); 
        }
    }
    
    public int MaxHunger
    {
        get => _maxHunger;
        set { int old = _maxHunger; _maxHunger = value; EmitSignal(SignalName.StatChanged, "MaxHunger", old, value); }
    }
    
    public int CurrentHunger
    {
        get => _currentHunger;
        set 
        { 
            int old = _currentHunger; 
            _currentHunger = Math.Clamp(value, 0, _maxHunger); 
            EmitSignal(SignalName.StatChanged, "CurrentHunger", old, _currentHunger); 
        }
    }
    
    public int MaxThirst
    {
        get => _maxThirst;
        set { int old = _maxThirst; _maxThirst = value; EmitSignal(SignalName.StatChanged, "MaxThirst", old, value); }
    }
    
    public int CurrentThirst
    {
        get => _currentThirst;
        set 
        { 
            int old = _currentThirst; 
            _currentThirst = Math.Clamp(value, 0, _maxThirst); 
            EmitSignal(SignalName.StatChanged, "CurrentThirst", old, _currentThirst); 
        }
    }

    public int Brutality
    {
        get => _brutality;
        set => _brutality = Math.Clamp(value, 0, 100);
    }
    
    public int Corruption
    {
        get => _corruption;
        set => _corruption = Math.Clamp(value, 0, 100);
    }
    
    public int Evil
    {
        get => _evil;
        set => _evil = Math.Clamp(value, 0, 100);
    }

    public int Draw { get; set; } = 5;
    public int HandLimit { get; set; } = 7;
    public int DeckCapacity { get; set; } = 30;

    [Export] public CharacterData? CharacterData { get; set; }

    public override void _Ready()
    {
        if (CharacterData != null)
        {
            InitializeFromData(CharacterData);
        }
    }

    public void InitializeFromData(CharacterData data)
    {
        CharacterData = data;
        _maxHp = data.MaxHp;
        _currentHp = data.MaxHp;
        _maxSanity = data.MaxSanity;
        _currentSanity = data.MaxSanity;
        _maxHunger = data.MaxHunger;
        _currentHunger = data.MaxHunger;
        _maxThirst = data.MaxThirst;
        _currentThirst = data.MaxThirst;
        Draw = data.Draw;
        HandLimit = data.HandLimit;
        DeckCapacity = data.DeckCapacity;
    }
}
