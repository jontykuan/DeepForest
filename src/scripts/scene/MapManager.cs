using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Core;

namespace DeepForest.Scene
{
    public partial class MapManager : Node
    {
        public static MapManager Instance { get; set; } = null!;

        public Dictionary<int, MapNode> Nodes { get; private set; } = new();
        public HashSet<int> ExploredNodeIds { get; private set; } = new();

        public SceneData? CurrentIndoorScene { get; set; } = null;

        private int _currentNodeId = 0;

        public int CurrentNodeId
        {
            get => _currentNodeId;
            set
            {
                _currentNodeId = value;
                ExploredNodeIds.Add(_currentNodeId);
                SceneEventHandler.OnPlayerEnterNode(_currentNodeId);
            }
        }

        public void LoadSetCurrentNodeId(int nodeId)
        {
            _currentNodeId = nodeId;
            ExploredNodeIds.Add(nodeId);
        }

        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            ExploredNodeIds.Clear();
            CurrentIndoorScene = null;
            MapGenerator.GenerateMap(Nodes, out int currentNodeId);
            _currentNodeId = currentNodeId;
            ExploredNodeIds.Add(_currentNodeId);
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
            sd.Actions.Add(new SceneAction { ActionName = "就地歇息", ThresholdType = ThresholdType.None, ThresholdValue = 0, EffectType = ActionEffectType.Camp });

            if (theme == "cabin")
            {
                sd.Actions.Add(new SceneAction { ActionName = "搜索室內", ThresholdType = ThresholdType.Wis, ThresholdValue = 3, EffectType = ActionEffectType.Search });
                sd.Actions.Add(new SceneAction { ActionName = "撬開地窖", ThresholdType = ThresholdType.Str, ThresholdValue = 6, EffectType = ActionEffectType.PryCellar });
            }
            else if (theme == "ruins")
            {
                sd.Actions.Add(new SceneAction { ActionName = "搜索遺跡室內", ThresholdType = ThresholdType.Wis, ThresholdValue = 3, EffectType = ActionEffectType.Search });
            }
            else if (theme == "cave")
            {
                sd.Actions.Add(new SceneAction { ActionName = "搜索洞穴深處", ThresholdType = ThresholdType.Wis, ThresholdValue = 3, EffectType = ActionEffectType.Search });
            }

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
    }
}
