using System;
using System.Collections.Generic;
using DeepForest.Core;

namespace DeepForest.Scene
{
    public static class MapGenerator
    {
        public static void GenerateMap(Dictionary<int, MapNode> nodes, out int currentNodeId)
        {
            var rand = new Random();
            nodes.Clear();

            int[] yCoords = { 450, 360, 270, 180, 90 };

            var startScene = new SceneData
            {
                SceneName = "紮營地",
                SceneDescription = "這裡是你的起點。營帳外籠罩著不祥的濃霧。",
                BottomGround = "dirt",
                LeftTerrain = "woodland",
                RightTerrain = "woodland"
            };
            startScene.Decals.Add("tent_left");
            ActionGenerator.GenerateDynamicActions(startScene, 0);

            var startNode = new MapNode { Id = 0, Depth = 0, Name = "紮營地", SceneData = startScene, X = 12, Y = yCoords[0] };
            nodes[0] = startNode;

            List<List<MapNode>> layers = new() { new List<MapNode> { startNode } };
            int currentId = 1;

            string[] grounds = { "dirt", "grass", "planks" };
            string[] terrains = { "woodland", "riverside", "swamp", "stone_wall", "ruins", "cabin" };
            string[] decalPool = { 
                "window", "door", "sofa", "npc", "chest", 
                "chest_wood", "chest_iron", "chest_cursed", 
                "door_normal", "door_strange", "door_cave", 
                "npc_hunter", "combat_wolf", "combat_cultist" 
            };

            for (int depth = 1; depth <= 3; depth++)
            {
                int numNodes = rand.Next(2, 4);
                var layerNodes = new List<MapNode>();

                for (int i = 0; i < numNodes; i++)
                {
                    string leftT = terrains[rand.Next(terrains.Length)];
                    string rightT = terrains[rand.Next(terrains.Length)];
                    if (leftT == "riverside" && rightT == "riverside")
                    {
                        if (rand.Next(2) == 0)
                            leftT = "woodland";
                        else
                            rightT = "woodland";
                    }
                    string ground = grounds[rand.Next(grounds.Length)];

                    string name = GetSceneNameFromTerrains(leftT, rightT);

                    var sd = new SceneData
                    {
                        SceneName = name,
                        SceneDescription = GetSceneDescriptionFromTerrains(leftT, rightT),
                        BottomGround = ground,
                        LeftTerrain = leftT,
                        RightTerrain = rightT
                    };

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

                    ActionGenerator.GenerateDynamicActions(sd, -1);

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
                    nodes[node.Id] = node;
                    layerNodes.Add(node);
                }
                layers.Add(layerNodes);
            }

            var exitScene = new SceneData
            {
                SceneName = "迷霧出口",
                SceneDescription = "迷霧在前方逐漸稀疏。這就是出口嗎？",
                BottomGround = "dirt",
                LeftTerrain = "woodland",
                RightTerrain = "woodland"
            };
            var exitNode = new MapNode { Id = currentId++, Depth = 4, Name = "迷霧出口", SceneData = exitScene, X = 12, Y = yCoords[4] };
            nodes[exitNode.Id] = exitNode;
            layers.Add(new List<MapNode> { exitNode });

            ActionGenerator.GenerateDynamicActions(exitScene, exitNode.Id);

            for (int d = 0; d < 4; d++)
            {
                var currentLayer = layers[d];
                var nextLayer = layers[d + 1];

                foreach (var u in currentLayer)
                {
                    int nextIdx = rand.Next(nextLayer.Count);
                    u.Connections.Add(nextLayer[nextIdx].Id);
                }

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

            currentNodeId = 0;
        }

        private static string GetSceneNameFromTerrains(string left, string right)
        {
            string lName = MapTerrainName(left);
            string rName = MapTerrainName(right);
            if (lName == rName) return $"雙生{lName}";
            return $"{lName}{rName}";
        }

        private static string MapTerrainName(string t) => t switch
        {
            "woodland" => "樹林",
            "riverside" => "河畔",
            "swamp" => "沼澤",
            "stone_wall" => "岩壁",
            "ruins" => "遺跡",
            "cabin" => "小屋",
            _ => "林地"
        };

        private static string GetSceneDescriptionFromTerrains(string left, string right)
        {
            return $"左側是荒涼的{MapTerrainName(left)}，右側是靜謐的{MapTerrainName(right)}。迷霧在中間翻湧。";
        }

        private static bool IsDecalAllowedInTerrain(string decalType, string left, string right)
        {
            return IsDecalAllowedSingle(decalType, left) || IsDecalAllowedSingle(decalType, right);
        }

        private static bool IsDecalAllowedSingle(string decalType, string terrain)
        {
            return decalType switch
            {
                "sofa" or "window" => (terrain == "cabin" || terrain == "ruins"),
                "door_normal" or "door_strange" => (terrain == "woodland" || terrain == "stone_wall" || terrain == "ruins" || terrain == "cabin"),
                "door_cave" => (terrain == "stone_wall" || terrain == "swamp" || terrain == "ruins"),
                "combat_wolf" => (terrain == "woodland" || terrain == "swamp" || terrain == "riverside"),
                "combat_ghost" => (terrain == "ruins" || terrain == "swamp" || terrain == "stone_wall"),
                "combat_cultist" => (terrain == "ruins" || terrain == "stone_wall" || terrain == "cabin"),
                "npc_hunter" => (terrain == "woodland" || terrain == "riverside" || terrain == "cabin"),
                "npc_witch" => (terrain == "swamp" || terrain == "ruins" || terrain == "stone_wall"),
                "chest_wood" or "chest_iron" or "chest_cursed" => true,
                _ => true
            };
        }
    }
}
