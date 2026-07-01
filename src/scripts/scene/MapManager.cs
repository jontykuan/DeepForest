using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Core;
using DeepForest.Cards;
using DeepForest.Combat;

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
    public SceneData? CurrentIndoorScene { get; set; }

    private int _currentNodeId = 0;
    public int CurrentNodeId 
    { 
        get => _currentNodeId; 
        set 
        { 
            _currentNodeId = value; 
            ExploredNodeIds.Add(value); 
            OnPlayerEnterNode(value);
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
            SceneName = "紮營地",
            SceneDescription = "這裡是你的起點。營帳外籠罩著不祥的濃霧。",
            BottomGround = "dirt",
            LeftTerrain = "woodland",
            RightTerrain = "woodland"
        };
        startScene.Decals.Add("tent_left");
        GenerateDynamicActions(startScene, isStart: true, isExit: false);

        var startNode = new MapNode { Id = 0, Depth = 0, Name = "紮營地", SceneData = startScene, X = 12, Y = yCoords[0] };
        Nodes[0] = startNode;
        layers.Add(new List<MapNode> { startNode });

        int currentId = 1;

        // 可選的地形與地面環境
        string[] grounds = { "dirt", "grass", "planks" };
        string[] terrains = { "woodland", "riverside", "swamp", "stone_wall", "ruins", "cabin" };
        string[] decalPool = { 
            "window", "door", "sofa", "npc", "chest", 
            "chest_wood", "chest_iron", "chest_cursed", 
            "door_normal", "door_strange", "door_cave", 
            "npc_hunter", "combat_wolf" 
        };

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

                // 隨機加入 0~2 個貼圖 (Decals) - 基於場景相容性過濾
                int numDecals = rand.Next(0, 3);
                for (int dIdx = 0; dIdx < numDecals; dIdx++)
                {
                    string decalType = decalPool[rand.Next(decalPool.Length)];
                    if (!IsDecalAllowedInTerrain(decalType, leftT, rightT))
                    {
                        continue;
                    }
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
            sd.Actions.Add(new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Any, ThresholdValue = 1, EffectType = ActionEffectType.MoveForward });
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
                case "chest_wood":
                    sd.Actions.Add(new SceneAction { ActionName = $"開啟{side}木箱", ThresholdType = ThresholdType.None, ThresholdValue = 0, EffectType = ActionEffectType.OpenWoodChest });
                    break;
                case "chest_iron":
                    sd.Actions.Add(new SceneAction { ActionName = $"撬開{side}鐵箱", ThresholdType = ThresholdType.Str, ThresholdValue = 4, EffectType = ActionEffectType.OpenIronChest });
                    break;
                case "chest_cursed":
                    sd.Actions.Add(new SceneAction { ActionName = $"觸摸{side}刻印箱", ThresholdType = ThresholdType.Wis, ThresholdValue = 3, EffectType = ActionEffectType.TouchCursedChest });
                    break;
                case "door_normal":
                    sd.Actions.Add(new SceneAction { ActionName = $"進入{side}木屋", ThresholdType = ThresholdType.None, ThresholdValue = 0, EffectType = ActionEffectType.EnterNormalCabin });
                    break;
                case "door_strange":
                    sd.Actions.Add(new SceneAction { ActionName = $"推開{side}血色門", ThresholdType = ThresholdType.Wis, ThresholdValue = 2, EffectType = ActionEffectType.EnterStrangeCabin });
                    break;
                case "door_cave":
                    sd.Actions.Add(new SceneAction { ActionName = $"爬入{side}洞穴", ThresholdType = ThresholdType.Dex, ThresholdValue = 3, EffectType = ActionEffectType.EnterCave });
                    break;
                case "npc_hunter":
                    sd.Actions.Add(new SceneAction { ActionName = $"與{side}獵人交易", ThresholdType = ThresholdType.Wis, ThresholdValue = 1, EffectType = ActionEffectType.TradeHunter });
                    break;
                case "npc_witch":
                    sd.Actions.Add(new SceneAction { ActionName = $"接受{side}魔女儀式", ThresholdType = ThresholdType.Wis, ThresholdValue = 4, EffectType = ActionEffectType.WitchRitual });
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

    public SceneData GenerateIndoorScene(int depth)
    {
        var rand = new Random();
        var sd = new SceneData();

        string theme = "cabin";
        if (Nodes.TryGetValue(GameState.Instance.EntranceNodeId, out var entranceNode))
        {
            if (entranceNode.Name.Contains("遺跡") || entranceNode.SceneData.LeftTerrain == "ruins" || entranceNode.SceneData.RightTerrain == "ruins")
                theme = "ruins";
            else if (entranceNode.Name.Contains("洞穴") || entranceNode.Name.Contains("深處"))
                theme = "cave";
        }

        sd.BottomGround = theme == "cabin" ? "planks" : "stone_tiles";
        sd.LeftTerrain = theme == "cabin" ? "cabin" : (theme == "ruins" ? "ruins" : "stone_wall");
        sd.RightTerrain = sd.LeftTerrain;

        string sideName = theme == "cabin" ? "小屋" : (theme == "ruins" ? "遺跡" : "洞穴");
        sd.SceneName = $"{sideName}深處 (深度 {depth})";
        sd.SceneDescription = $"你正處於{sideName}內部的漆黑環境中。寒意刺骨，前方深不可測。";

        bool spawnExit = false;
        if (depth >= 5)
        {
            spawnExit = true;
        }
        else if (depth >= 3)
        {
            spawnExit = rand.NextDouble() < 0.4;
        }

        if (spawnExit)
        {
            sd.Decals.Add("door_center");
            sd.SceneDescription += " 前方的黑暗通道中，隱約露出了微弱的外部亮光！";
        }
        else
        {
            int roll = rand.Next(0, 4);
            if (roll == 0) sd.Decals.Add("chest_left");
            else if (roll == 1) sd.Decals.Add("sofa_right");
            else if (roll == 2) sd.Decals.Add("npc_right");
        }

        sd.Actions.Add(new SceneAction { ActionName = "探索深處", ThresholdType = ThresholdType.Dex, ThresholdValue = 2, EffectType = ActionEffectType.ExploreIndoor });

        if (depth <= 3)
        {
            sd.Actions.Add(new SceneAction { ActionName = "原路返回室外", ThresholdType = ThresholdType.None, ThresholdValue = 0, EffectType = ActionEffectType.ReturnOutdoor });
        }

        if (spawnExit)
        {
            sd.Actions.Add(new SceneAction { ActionName = "從出口逃離室內", ThresholdType = ThresholdType.None, ThresholdValue = 0, EffectType = ActionEffectType.LeaveIndoor });
        }

        return sd;
    }

    public int GetRandomDownstreamNode(int startId, int steps)
    {
        var rand = new Random();
        int currentId = startId;

        for (int i = 0; i < steps; i++)
        {
            if (Nodes.TryGetValue(currentId, out var node) && node.Connections.Count > 0)
            {
                int nextIdx = rand.Next(node.Connections.Count);
                currentId = node.Connections[nextIdx];
            }
            else
            {
                break;
            }
        }
        return currentId;
    }

    private void OnPlayerEnterNode(int nodeId)
    {
        if (nodeId == 0) return;
        var node = Nodes[nodeId];
        var sd = node.SceneData;
        var player = GameState.Instance.PlayerInstance;
        var deck = GameState.Instance.DeckInstance;

        // 1. 低理智幻覺事件 (Sanity < 30)
        if (player.CurrentSanity < 30)
        {
            if (!sd.Decals.Contains("combat_ghost_left"))
            {
                sd.Decals.Add("combat_ghost_left");
                GameState.Instance.AddLog("【幻覺】在稀薄的理智中，你隱約看見左側迷霧裡晃動著怨靈的陰影...");
            }
        }
        else
        {
            sd.Decals.Remove("combat_ghost_left");
        }

        // 2. 牌組持有「帶血的日記」觸發神秘 NPC 事件
        bool hasDiary = deck.Hand.Exists(c => c.CardName == "帶血的日記") || 
                        deck.DiscardPile.Exists(c => c.CardName == "帶血的日記") ||
                        deck.DrawPile.Exists(c => c.CardName == "帶血的日記") ||
                        deck.EquippedCards.Exists(c => c.CardName == "帶血的日記");

        if (hasDiary)
        {
            if (!sd.Decals.Contains("npc_witch_right"))
            {
                sd.Decals.Add("npc_witch_right");
                GameState.Instance.AddLog("身上的日記隱隱發熱，一名神祕的魔女出現在右側，靜靜地看著你。");
            }
        }

        // 重新生成行動
        bool isExit = (nodeId == Nodes.Count - 1);
        GenerateDynamicActions(sd, isStart: false, isExit: isExit);

        // 3. 檢查是否有戰鬥貼圖並自動觸發戰鬥
        foreach (var decal in sd.Decals)
        {
            if (decal.StartsWith("combat_wolf"))
            {
                var enemyData = GetEnemyDataForDecal("combat_wolf");
                CombatManager.Instance.StartCombat(enemyData);
                break;
            }
            else if (decal.StartsWith("combat_ghost"))
            {
                var enemyData = GetEnemyDataForDecal("combat_ghost");
                CombatManager.Instance.StartCombat(enemyData);
                break;
            }
        }
    }

    private EnemyData GetEnemyDataForDecal(string decalType)
    {
        var enemy = new EnemyData();
        if (decalType == "combat_wolf")
        {
            enemy.EnemyName = "野狼";
            enemy.MaxHp = 3;
            enemy.AttackPower = 10;
            enemy.IsAggressive = true;
            enemy.HideHp = false;
            enemy.DecalName = "combat_wolf_right";
            
            enemy.ActionDeck.Add(new Card { CardName = "撕咬", CardType = CardType.ActionStr, StrValue = 2 });
            enemy.ActionDeck.Add(new Card { CardName = "猛撲", CardType = CardType.ActionDex, DexValue = 3 });
            enemy.ActionDeck.Add(new Card { CardName = "試探", CardType = CardType.ActionWis, WisValue = 1 });

            enemy.LootTable.Add(new Card { CardName = "狼肉", CardType = CardType.Consumable, Description = "新鮮帶血的狼肉。", HpCost = -8, HungerCost = -20 });
            enemy.LootTable.Add(new Card { CardName = "狼皮", CardType = CardType.Equipment, Description = "保暖粗糙的狼皮。", Weight = 2 });
        }
        else if (decalType == "combat_ghost")
        {
            enemy.EnemyName = "怨靈";
            enemy.MaxHp = 4;
            enemy.AttackPower = 15;
            enemy.IsAggressive = false;
            enemy.HideHp = true;
            enemy.DecalName = "combat_ghost_left";

            enemy.ActionDeck.Add(new Card { CardName = "陰冷低語", CardType = CardType.ActionWis, WisValue = 3 });
            enemy.ActionDeck.Add(new Card { CardName = "鬼影重重", CardType = CardType.ActionDex, DexValue = 2 });
        }
        return enemy;
    }

    private bool IsDecalAllowedInTerrain(string decalType, string left, string right)
    {
        return IsDecalAllowedSingle(decalType, left) || IsDecalAllowedSingle(decalType, right);
    }

    private bool IsDecalAllowedSingle(string decalType, string terrain)
    {
        return decalType switch
        {
            "sofa" or "window" => (terrain == "cabin" || terrain == "ruins"),
            "door_normal" or "door_strange" => (terrain == "woodland" || terrain == "stone_wall" || terrain == "ruins" || terrain == "cabin"),
            "door_cave" => (terrain == "stone_wall" || terrain == "swamp" || terrain == "ruins"),
            "combat_wolf" => (terrain == "woodland" || terrain == "swamp" || terrain == "riverside"),
            "combat_ghost" => (terrain == "ruins" || terrain == "swamp" || terrain == "stone_wall"),
            "npc_hunter" => (terrain == "woodland" || terrain == "riverside" || terrain == "cabin"),
            "npc_witch" => (terrain == "swamp" || terrain == "ruins" || terrain == "stone_wall"),
            "chest_wood" or "chest_iron" or "chest_cursed" => true,
            _ => true
        };
    }
}
