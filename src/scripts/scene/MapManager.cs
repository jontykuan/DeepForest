using Godot;
using System;
using System.Collections.Generic;

namespace DeepForest.Scene;

public class MapNode
{
    public int Id { get; set; }
    public int Depth { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
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
        ExploredNodeIds.Clear();
        ExploredNodeIds.Add(0);

        var rand = new Random();
        List<List<MapNode>> layers = new();

        // Y 座標對應深度 (0=起點, 1~3=隨機, 4=出口)
        int[] yCoords = { 10, 7, 5, 2, 0 };

        // Layer 0: 起點營地
        var startScene = new SceneData
        {
            SceneName = "野營帳篷",
            SceneDescription = "這裡是你的起點。營帳外籠罩著不祥的濃霧。",
            BottomGround = "dirt",
            LeftTerrain = "woodland",
            RightTerrain = "woodland"
        };
        startScene.Decals.Add("tent_left");
        GenerateDynamicActions(startScene, isStart: true, isExit: false);

        var startNode = new MapNode { Id = 0, Depth = 0, Name = "野營帳篷", SceneData = startScene, X = 12, Y = yCoords[0] };
        Nodes[0] = startNode;
        layers.Add(new List<MapNode> { startNode });

        int currentId = 1;

        // 可選的地形與地面環境
        string[] grounds = { "dirt", "grass", "planks" };
        string[] terrains = { "woodland", "riverside", "swamp", "stone_wall", "ruins", "cabin" };
        string[] decalPool = { "window", "door", "sofa", "npc", "chest" };

        // Layer 1~3: 隨機關卡
        for (int depth = 1; depth <= 3; depth++)
        {
            int numNodes = rand.Next(2, 4); // 每一層生成 2 或 3 個節點
            var layerNodes = new List<MapNode>();

            for (int i = 0; i < numNodes; i++)
            {
                string leftT = terrains[rand.Next(terrains.Length)];
                string rightT = terrains[rand.Next(terrains.Length)];
                string ground = grounds[rand.Next(grounds.Length)];

                // 根據左右地形動態決定名字
                string name = GetSceneNameFromTerrains(leftT, rightT);

                var sd = new SceneData
                {
                    SceneName = name,
                    SceneDescription = GetSceneDescriptionFromTerrains(leftT, rightT),
                    BottomGround = ground,
                    LeftTerrain = leftT,
                    RightTerrain = rightT
                };

                // 隨機加入 0~2 個貼圖 (Decals)
                int numDecals = rand.Next(0, 3);
                for (int dIdx = 0; dIdx < numDecals; dIdx++)
                {
                    string decalType = decalPool[rand.Next(decalPool.Length)];
                    string side = rand.Next(2) == 0 ? "left" : "right";
                    sd.Decals.Add($"{decalType}_{side}");
                }

                // 動態生成與地形/貼圖相關聯的行動
                GenerateDynamicActions(sd, isStart: false, isExit: false);

                int centerX = 15;
                if (numNodes == 2)
                {
                    centerX = (i == 0) ? 8 : 22;
                }
                else if (numNodes == 3)
                {
                    centerX = (i == 0) ? 6 : ((i == 1) ? 15 : 24);
                }

                var node = new MapNode
                {
                    Id = currentId++,
                    Depth = depth,
                    Name = name,
                    SceneData = sd,
                    X = centerX - 3,
                    Y = yCoords[depth]
                };
                Nodes[node.Id] = node;
                layerNodes.Add(node);
            }
            layers.Add(layerNodes);
        }

        // Layer 4: 出口
        var exitScene = new SceneData
        {
            SceneName = "迷霧出口",
            SceneDescription = "迷霧在前方逐漸稀疏。這就是出口嗎？",
            BottomGround = "dirt",
            LeftTerrain = "woodland",
            RightTerrain = "woodland"
        };
        GenerateDynamicActions(exitScene, isStart: false, isExit: true);

        var exitNode = new MapNode { Id = currentId++, Depth = 4, Name = "迷霧出口", SceneData = exitScene, X = 12, Y = yCoords[4] };
        Nodes[exitNode.Id] = exitNode;
        layers.Add(new List<MapNode> { exitNode });

        // 連接相鄰層級的節點
        for (int d = 0; d < 4; d++)
        {
            var currentLayer = layers[d];
            var nextLayer = layers[d + 1];

            // 確保目前層的每個節點都至少連向下一層的一個節點
            foreach (var u in currentLayer)
            {
                int nextIdx = rand.Next(nextLayer.Count);
                u.Connections.Add(nextLayer[nextIdx].Id);
            }

            // 確保下一層的每個節點都有來自上一層的連線（防止孤立）
            foreach (var v in nextLayer)
            {
                bool hasParent = false;
                foreach (var u in currentLayer)
                {
                    if (u.Connections.Contains(v.Id))
                    {
                        hasParent = true;
                        break;
                    }
                }

                if (!hasParent)
                {
                    int parentIdx = rand.Next(currentLayer.Count);
                    var parentNode = currentLayer[parentIdx];
                    if (!parentNode.Connections.Contains(v.Id))
                    {
                        parentNode.Connections.Add(v.Id);
                    }
                }
            }
        }

        CurrentNodeId = 0;
    }

    private string GetSceneNameFromTerrains(string left, string right)
    {
        string lName = MapTerrainName(left);
        string rName = MapTerrainName(right);
        if (lName == rName) return $"雙生{lName}";
        return $"{lName}{rName}";
    }

    private string MapTerrainName(string t) => t switch
    {
        "woodland" => "樹林",
        "riverside" => "河畔",
        "swamp" => "沼澤",
        "stone_wall" => "岩壁",
        "ruins" => "遺跡",
        "cabin" => "小屋",
        _ => "林地"
    };

    private string GetSceneDescriptionFromTerrains(string left, string right)
    {
        return $"左側是荒涼的{MapTerrainName(left)}，右側是靜謐的{MapTerrainName(right)}。迷霧在中間翻湧。";
    }

    private void GenerateDynamicActions(SceneData sd, bool isStart, bool isExit)
    {
        sd.Actions.Clear();

        if (isStart)
        {
            sd.Actions.Add(new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Dex, ThresholdValue = 1, EffectType = ActionEffectType.MoveForward });
            return;
        }

        if (isExit)
        {
            sd.Actions.Add(new SceneAction { ActionName = "逃離森林", RequiredItem = "地圖殘片", EffectType = ActionEffectType.MoveForward });
            return;
        }

        // 根據左/右地形動態加入行動
        AddTerrainActions(sd, sd.LeftTerrain, "左側");
        AddTerrainActions(sd, sd.RightTerrain, "右側");

        // 根據貼圖動態加入行動
        foreach (var decal in sd.Decals)
        {
            var parts = decal.Split('_');
            if (parts.Length < 2) continue;
            string type = parts[0];
            string side = parts[1] == "left" ? "左側" : "右側";

            switch (type)
            {
                case "window":
                    sd.Actions.Add(new SceneAction { ActionName = $"窺視{side}窗戶", ThresholdType = ThresholdType.Wis, ThresholdValue = 2, EffectType = ActionEffectType.Search });
                    break;
                case "door":
                    sd.Actions.Add(new SceneAction { ActionName = $"拉開{side}木門", ThresholdType = ThresholdType.Str, ThresholdValue = 3, EffectType = ActionEffectType.Search });
                    break;
                case "sofa":
                    sd.Actions.Add(new SceneAction { ActionName = $"在{side}沙發小憩", ThresholdType = ThresholdType.Str, ThresholdValue = 1, EffectType = ActionEffectType.Camp });
                    break;
                case "npc":
                    sd.Actions.Add(new SceneAction { ActionName = $"與{side}陌生人交談", ThresholdType = ThresholdType.Wis, ThresholdValue = 1, EffectType = ActionEffectType.Search });
                    break;
                case "chest":
                    sd.Actions.Add(new SceneAction { ActionName = $"撬開{side}寶箱", ThresholdType = ThresholdType.Str, ThresholdValue = 4, EffectType = ActionEffectType.Search });
                    break;
            }
        }

        // 必備的前進選項
        sd.Actions.Add(new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Dex, ThresholdValue = 2, EffectType = ActionEffectType.MoveForward });
    }

    private void AddTerrainActions(SceneData sd, string terrain, string side)
    {
        switch (terrain)
        {
            case "riverside":
                // 捕魚與裝水（避免重複加入）
                if (!HasActionName(sd, "捕魚"))
                    sd.Actions.Add(new SceneAction { ActionName = "捕魚", ThresholdType = ThresholdType.Dex, ThresholdValue = 3, EffectType = ActionEffectType.Fish });
                if (!HasActionName(sd, "裝水"))
                    sd.Actions.Add(new SceneAction { ActionName = "裝水", RequiredItem = "水瓶", EffectType = ActionEffectType.CollectWater });
                break;
            case "cabin":
            case "ruins":
                string place = terrain == "cabin" ? "小屋" : "遺跡";
                if (!HasActionName(sd, $"搜索{place}"))
                    sd.Actions.Add(new SceneAction { ActionName = $"搜索{place}", ThresholdType = ThresholdType.Wis, ThresholdValue = 3, EffectType = ActionEffectType.Search });
                if (!HasActionName(sd, "紮營"))
                    sd.Actions.Add(new SceneAction { ActionName = "紮營", ThresholdType = ThresholdType.Str, ThresholdValue = 2, EffectType = ActionEffectType.Camp });
                if (terrain == "cabin" && !HasActionName(sd, "撬開地窖"))
                    sd.Actions.Add(new SceneAction { ActionName = "撬開地窖", ThresholdType = ThresholdType.Str, ThresholdValue = 6, EffectType = ActionEffectType.PryCellar });
                break;
            case "woodland":
            case "swamp":
                string envName = terrain == "woodland" ? "樹林" : "沼澤";
                if (!HasActionName(sd, $"探索{side}{envName}"))
                    sd.Actions.Add(new SceneAction { ActionName = $"探索{side}{envName}", ThresholdType = ThresholdType.Wis, ThresholdValue = 2, EffectType = ActionEffectType.Search });
                break;
        }
    }

    private bool HasActionName(SceneData sd, string actionName)
    {
        foreach (var action in sd.Actions)
        {
            if (action.ActionName == actionName) return true;
        }
        return false;
    }
}
