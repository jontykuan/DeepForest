using System;
using System.Collections.Generic;
using DeepForest.Core;

namespace DeepForest.Scene
{
    public static class MapGenerator
    {
        public static void GenerateMap(Dictionary<int, MapNode> nodes, out int currentNodeId)
        {
            nodes.Clear();

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

            var startNode = new MapNode { Id = 0, Depth = 0, Name = "紮營地", SceneData = startScene, X = 12, Y = 10 };
            nodes[0] = startNode;

            currentNodeId = 0;
        }

        public static void EnsureNodeConnections(MapManager manager, int nodeId)
        {
            if (!manager.Nodes.TryGetValue(nodeId, out var node)) return;
            if (node.Connections.Count > 0) return;

            var rand = new Random();
            int numNodes = rand.Next(2, 4); // 2 or 3 branches
            int nextDepth = node.Depth + 1;

            string[] grounds = { "dirt", "grass", "planks" };
            string[] decalPool = { 
                "window", "door", "sofa", "npc", "chest", 
                "chest_wood", "chest_iron", "chest_cursed", 
                "door_normal", "door_strange", "door_cave", 
                "npc_hunter", "combat_wolf", "combat_cultist" 
            };

            int maxId = 0;
            foreach (var key in manager.Nodes.Keys)
            {
                if (key > maxId) maxId = key;
            }
            int nextIdStart = maxId + 1;

            for (int i = 0; i < numNodes; i++)
            {
                int childId = nextIdStart + i;
                string parentLeft = node.SceneData.LeftTerrain;
                string parentRight = node.SceneData.RightTerrain;

                string leftT = GetTransitionTerrain(parentLeft, rand);
                string rightT = GetTransitionTerrain(parentRight, rand);

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

                bool leftPassable = (leftT == "swamp" || leftT == "ruins");
                bool rightPassable = (rightT == "swamp" || rightT == "ruins");
                bool forwardPassable = rand.NextDouble() >= 0.3; // 70% chance forward is passable

                if (!leftPassable && !rightPassable && !forwardPassable)
                {
                    // Force at least one direction to have a passable event decal
                    int forceSide = rand.Next(0, 2); // 0 = left, 1 = right
                    string decalType = rand.Next(0, 3) switch
                    {
                        0 => "door_normal",
                        1 => "door_strange",
                        _ => "door_cave"
                    };
                    string sideStr = forceSide == 0 ? "left" : "right";
                    sd.Decals.Add($"{decalType}_{sideStr}");
                    sd.IsForwardBlocked = true;
                }
                else if (!forwardPassable)
                {
                    sd.IsForwardBlocked = true;
                }

                // If not forced, randomly populate decals
                if (sd.Decals.Count == 0)
                {
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
                }

                ActionGenerator.GenerateDynamicActions(sd, childId);

                int centerX = 15;
                if (numNodes == 2)
                {
                    centerX = (i == 0) ? 8 : 22;
                }
                else if (numNodes == 3)
                {
                    centerX = (i == 0) ? 6 : ((i == 1) ? 15 : 24);
                }

                var childNode = new MapNode
                {
                    Id = childId,
                    Depth = nextDepth,
                    Name = name,
                    SceneData = sd,
                    X = centerX - 3,
                    Y = node.Y - 2
                };
                manager.Nodes[childId] = childNode;
                node.Connections.Add(childId);
            }
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

        private static string GetTransitionTerrain(string current, Random rand)
        {
            var allowed = current switch
            {
                "woodland" => new[] { "woodland", "woodland", "riverside", "cabin" },
                "riverside" => new[] { "riverside", "riverside", "woodland", "swamp" },
                "swamp" => new[] { "swamp", "swamp", "riverside", "stone_wall" },
                "stone_wall" => new[] { "stone_wall", "stone_wall", "swamp", "ruins" },
                "ruins" => new[] { "ruins", "ruins", "stone_wall", "cabin" },
                "cabin" => new[] { "cabin", "cabin", "woodland", "ruins" },
                _ => new[] { "woodland", "riverside", "swamp", "stone_wall", "ruins", "cabin" }
            };
            return allowed[rand.Next(allowed.Length)];
        }
    }
}
