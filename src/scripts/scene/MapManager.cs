using Godot;
using System;
using System.Collections.Generic;

namespace DeepForest.Scene;

public class MapNode
{
    public int Id { get; set; }
    public int Depth { get; set; }
    public string Name { get; set; } = "";
    public SceneData SceneData { get; set; } = new();
    public List<int> Connections { get; set; } = new();
}

public partial class MapManager : Node
{
    public static MapManager Instance { get; private set; } = null!;

    public Dictionary<int, MapNode> Nodes { get; private set; } = new();
    public HashSet<int> ExploredNodeIds { get; private set; } = new() { 0 };

    private int _currentNodeId = 0;
    public int CurrentNodeId 
    { 
        get => _currentNodeId; 
        set 
        { 
            _currentNodeId = value; 
            ExploredNodeIds.Add(value); 
        } 
    }

    public override void _Ready()
    {
        Instance = this;
        GenerateMap();
    }

    public void GenerateMap()
    {
        Nodes.Clear();

        var campNode = new MapNode { Id = 0, Depth = 0, Name = "野營帳篷", SceneData = CreateCampSceneData() };
        Nodes[0] = campNode;

        var node1 = new MapNode { Id = 1, Depth = 1, Name = "幽暗河畔", SceneData = CreateRiverSceneData() };
        var node2 = new MapNode { Id = 2, Depth = 1, Name = "廢棄獵屋", SceneData = CreateCabinSceneData() };
        Nodes[1] = node1;
        Nodes[2] = node2;
        campNode.Connections.Add(1);
        campNode.Connections.Add(2);

        var node3 = new MapNode { Id = 3, Depth = 2, Name = "迷霧深處", SceneData = CreateForestSceneData() };
        var node4 = new MapNode { Id = 4, Depth = 2, Name = "荊棘祭壇", SceneData = CreateShrineSceneData() };
        Nodes[3] = node3;
        Nodes[4] = node4;
        node1.Connections.Add(3);
        node1.Connections.Add(4);
        node2.Connections.Add(4); 

        var node5 = new MapNode { Id = 5, Depth = 3, Name = "迷霧出口", SceneData = CreateExitSceneData() };
        Nodes[5] = node5;
        node3.Connections.Add(5);
        node4.Connections.Add(5);

        CurrentNodeId = 0;
    }

    private SceneData CreateCampSceneData()
    {
        var sd = new SceneData { SceneName = "野營帳篷", SceneDescription = "這裡是你的起點。營帳外籠罩著不祥的濃霧。" };
        var leave = new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Dex, ThresholdValue = 1, EffectType = ActionEffectType.MoveForward };
        sd.Actions.Add(leave);
        return sd;
    }

    private SceneData CreateRiverSceneData()
    {
        var sd = new SceneData { SceneName = "幽暗河畔", SceneDescription = "冰冷的水流在黑暗中靜靜流淌，發出詭異的聲響。" };
        
        var camp = new SceneAction { ActionName = "紮營", ThresholdType = ThresholdType.Str, ThresholdValue = 4, EffectType = ActionEffectType.Camp };
        var fish = new SceneAction { ActionName = "捕魚", ThresholdType = ThresholdType.Dex, ThresholdValue = 3, EffectType = ActionEffectType.Fish };
        var water = new SceneAction { ActionName = "裝水", RequiredItem = "水瓶", EffectType = ActionEffectType.CollectWater };
        var forward = new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Dex, ThresholdValue = 2, EffectType = ActionEffectType.MoveForward };

        sd.Actions.Add(camp);
        sd.Actions.Add(fish);
        sd.Actions.Add(water);
        sd.Actions.Add(forward);

        return sd;
    }

    private SceneData CreateCabinSceneData()
    {
        var sd = new SceneData { SceneName = "廢棄獵屋", SceneDescription = "一棟破舊不堪的獵人小屋，木板門正發出吱呀聲。" };
        
        var search = new SceneAction { ActionName = "搜索", ThresholdType = ThresholdType.Wis, ThresholdValue = 3, EffectType = ActionEffectType.Search };
        var camp = new SceneAction { ActionName = "紮營", ThresholdType = ThresholdType.Str, ThresholdValue = 2, EffectType = ActionEffectType.Camp };
        var pry = new SceneAction { ActionName = "撬開地窖", ThresholdType = ThresholdType.Str, ThresholdValue = 6, EffectType = ActionEffectType.PryCellar };
        var forward = new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Dex, ThresholdValue = 2, EffectType = ActionEffectType.MoveForward };

        sd.Actions.Add(search);
        sd.Actions.Add(camp);
        sd.Actions.Add(pry);
        sd.Actions.Add(forward);

        return sd;
    }

    private SceneData CreateForestSceneData()
    {
        var sd = new SceneData { SceneName = "迷霧深處", SceneDescription = "樹木像怪物的爪子一樣扭曲，林中回盪著莫名的低語。" };
        var forward = new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Dex, ThresholdValue = 3, EffectType = ActionEffectType.MoveForward };
        sd.Actions.Add(forward);
        return sd;
    }

    private SceneData CreateShrineSceneData()
    {
        var sd = new SceneData { SceneName = "荊棘祭壇", SceneDescription = "一個被荊棘纏繞的石製祭壇，散發著微弱的幽光。" };
        var forward = new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Wis, ThresholdValue = 4, EffectType = ActionEffectType.MoveForward };
        sd.Actions.Add(forward);
        return sd;
    }

    private SceneData CreateExitSceneData()
    {
        var sd = new SceneData { SceneName = "迷霧出口", SceneDescription = "迷霧在前方逐漸稀疏。這就是出口嗎？" };
        var escape = new SceneAction { ActionName = "逃離森林", RequiredItem = "地圖殘片", EffectType = ActionEffectType.MoveForward };
        sd.Actions.Add(escape);
        return sd;
    }

    public string GetMapAsciiRepresentation()
    {
        string map = "";
        map += "   [出口] (深度3)\n";
        map += "     /   \\\n";
        map += $" {(CurrentNodeId == 3 ? "[*深處]" : " [深處]")} {(CurrentNodeId == 4 ? "[*祭壇]" : " [祭壇]")}\n";
        map += "   |   \\ /   |\n";
        map += $" {(CurrentNodeId == 1 ? "[*河畔]" : " [河畔]")} {(CurrentNodeId == 2 ? "[*獵屋]" : " [獵屋]")}\n";
        map += "    \\   / \n";
        map += $"    {(CurrentNodeId == 0 ? "[*起點]" : " [起點]")}\n";
        return map;
    }
}
