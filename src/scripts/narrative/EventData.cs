using Godot;

namespace DeepForest.Narrative
{
    [GlobalClass]
    public partial class EventData : Resource
    {
        [Export] public string EventId { get; set; } = "event_default";
        [Export] public string EventTitle { get; set; } = "事件名稱";
        [Export(PropertyHint.MultilineText)] public string EventDescription { get; set; } = "事件描述...";
        [Export] public int Weight { get; set; } = 10;

        // Trigger conditions
        [Export] public string RequiredTerrain { get; set; } = "";
        [Export] public string RequiredWeather { get; set; } = "";
        [Export] public int MinDepth { get; set; } = 0;
        [Export] public int MaxDepth { get; set; } = 999;
        [Export] public Character.CharacterId RequiredCharacterId { get; set; } = Character.CharacterId.None;
        
        [Export] public int MinSanity { get; set; } = 0;
        [Export] public int MaxSanity { get; set; } = 999;
        [Export] public int MinHp { get; set; } = 0;
        [Export] public int MaxHp { get; set; } = 999;

        [Export] public int MinDay { get; set; } = 1;
        [Export] public int MaxDay { get; set; } = 999;
        [Export] public int MinBrutality { get; set; } = 0;
        [Export] public int MaxBrutality { get; set; } = 999;
        [Export] public int MinCorruption { get; set; } = 0;
        [Export] public int MaxCorruption { get; set; } = 999;
        [Export] public int MinEvil { get; set; } = 0;
        [Export] public int MaxEvil { get; set; } = 999;
        [Export] public Cards.CardId RequiredCardId { get; set; } = Cards.CardId.None;

        // Rendered decal
        [Export] public string DecalName { get; set; } = "npc_right";

        // Options
        [Export] public Godot.Collections.Array<EventOption> Options { get; set; } = new();
    }
}
