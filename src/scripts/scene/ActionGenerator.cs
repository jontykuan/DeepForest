using System;
using System.Collections.Generic;
using System.Linq;
using DeepForest.Core;
using DeepForest.Narrative;
using DeepForest.Cards;

namespace DeepForest.Scene
{
    public static class ActionGenerator
    {
        public static void GenerateDynamicActions(SceneData sd, int nodeId)
        {
            sd.Actions.Clear();

            if (nodeId == 0)
            {
                sd.Actions.Add(new SceneAction { ActionName = "紮營", ThresholdType = ThresholdType.Str, ThresholdValue = 2, EffectType = ActionEffectType.Camp });
                sd.Actions.Add(new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Dex, ThresholdValue = 2, EffectType = ActionEffectType.MoveForward });
                return;
            }

            var mapManager = MapManager.Instance;
            bool isExit = (nodeId == mapManager.Nodes.Count - 1);
            if (isExit && nodeId != -1)
            {
                sd.Actions.Add(new SceneAction { ActionName = "逃離森林", RequiredItem = "", EffectType = ActionEffectType.MoveForward });
                return;
            }

            AddTerrainActions(sd, sd.LeftTerrain, "左側");
            AddTerrainActions(sd, sd.RightTerrain, "右側");

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
                        sd.Actions.Add(new SceneAction { ActionName = $"拉開{side}木門", ThresholdType = ThresholdType.None, ThresholdValue = 0, EffectType = ActionEffectType.OpenNormalCabinDoor });
                        sd.Actions.Add(new SceneAction { ActionName = $"撬開{side}木門", ThresholdType = ThresholdType.Str, ThresholdValue = 3, EffectType = ActionEffectType.OpenNormalCabinDoor });
                        break;
                    case "door_strange":
                        sd.Actions.Add(new SceneAction { ActionName = $"推開{side}血色門", ThresholdType = ThresholdType.Wis, ThresholdValue = 2, EffectType = ActionEffectType.OpenStrangeCabinDoor });
                        break;
                    case "door_cave":
                        sd.Actions.Add(new SceneAction { ActionName = $"探索{side}洞穴入口", ThresholdType = ThresholdType.Dex, ThresholdValue = 3, EffectType = ActionEffectType.OpenCaveEntrance });
                        break;
                    case "npc_hunter":
                        sd.Actions.Add(new SceneAction { ActionName = $"與{side}獵人交易", ThresholdType = ThresholdType.Wis, ThresholdValue = 1, EffectType = ActionEffectType.TradeHunter });
                        break;
                    case "npc_witch":
                        sd.Actions.Add(new SceneAction { ActionName = $"接受{side}魔女儀式", ThresholdType = ThresholdType.Wis, ThresholdValue = 4, EffectType = ActionEffectType.WitchRitual });
                        break;
                }
            }

            if (nodeId != -1 && mapManager.Nodes.TryGetValue(nodeId, out var node))
            {
                var sortedConns = node.Connections.OrderBy(id => mapManager.Nodes[id].X).ToList();
                if (sortedConns.Count == 1)
                {
                    if (!HasActionName(sd, "前進"))
                        sd.Actions.Add(new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Dex, ThresholdValue = 2, EffectType = ActionEffectType.MoveForward });
                }
                else if (sortedConns.Count == 2)
                {
                    bool leftPassable = IsTerrainPassable(sd.LeftTerrain, out int leftWis, out int leftDex);
                    bool rightPassable = IsTerrainPassable(sd.RightTerrain, out int rightWis, out int rightDex);

                    if (leftPassable)
                    {
                        string actionName = sd.LeftTerrain == "swamp" ? "向左側穿越沼澤" : "向左側進入遺跡";
                        ThresholdType tt = sd.LeftTerrain == "swamp" ? ThresholdType.Dex : ThresholdType.Wis;
                        int tv = sd.LeftTerrain == "swamp" ? leftDex : leftWis;
                        if (!HasActionName(sd, actionName))
                            sd.Actions.Add(new SceneAction { ActionName = actionName, ThresholdType = tt, ThresholdValue = tv, EffectType = ActionEffectType.MoveForward });
                    }
                    if (rightPassable)
                    {
                        string actionName = sd.RightTerrain == "swamp" ? "向右側穿越沼澤" : "向右側進入遺跡";
                        ThresholdType tt = sd.RightTerrain == "swamp" ? ThresholdType.Dex : ThresholdType.Wis;
                        int tv = sd.RightTerrain == "swamp" ? rightDex : rightWis;
                        if (!HasActionName(sd, actionName))
                            sd.Actions.Add(new SceneAction { ActionName = actionName, ThresholdType = tt, ThresholdValue = tv, EffectType = ActionEffectType.MoveForward });
                    }

                    if (!leftPassable && !rightPassable)
                    {
                        if (!HasActionName(sd, "前進"))
                            sd.Actions.Add(new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Dex, ThresholdValue = 2, EffectType = ActionEffectType.MoveForward });
                    }
                }
                else if (sortedConns.Count >= 3)
                {
                    bool leftPassable = IsTerrainPassable(sd.LeftTerrain, out int leftWis, out int leftDex);
                    bool rightPassable = IsTerrainPassable(sd.RightTerrain, out int rightWis, out int rightDex);

                    if (leftPassable)
                    {
                        string actionName = sd.LeftTerrain == "swamp" ? "向左側穿越沼澤" : "向左側進入遺跡";
                        ThresholdType tt = sd.LeftTerrain == "swamp" ? ThresholdType.Dex : ThresholdType.Wis;
                        int tv = sd.LeftTerrain == "swamp" ? leftDex : leftWis;
                        if (!HasActionName(sd, actionName))
                            sd.Actions.Add(new SceneAction { ActionName = actionName, ThresholdType = tt, ThresholdValue = tv, EffectType = ActionEffectType.MoveForward });
                    }

                    if (!HasActionName(sd, "前進"))
                        sd.Actions.Add(new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Dex, ThresholdValue = 2, EffectType = ActionEffectType.MoveForward });

                    if (rightPassable)
                    {
                        string actionName = sd.RightTerrain == "swamp" ? "向右側穿越沼澤" : "向右側進入遺跡";
                        ThresholdType tt = sd.RightTerrain == "swamp" ? ThresholdType.Dex : ThresholdType.Wis;
                        int tv = sd.RightTerrain == "swamp" ? rightDex : rightWis;
                        if (!HasActionName(sd, actionName))
                            sd.Actions.Add(new SceneAction { ActionName = actionName, ThresholdType = tt, ThresholdValue = tv, EffectType = ActionEffectType.MoveForward });
                    }
                }
            }
            else
            {
                if (!HasActionName(sd, "前進"))
                    sd.Actions.Add(new SceneAction { ActionName = "前進", ThresholdType = ThresholdType.Dex, ThresholdValue = 2, EffectType = ActionEffectType.MoveForward });
            }

            bool canCamp = !GameState.Instance.IsDescentActive || sd.Decals.Contains("campfire");

            if (nodeId != 0 && !isExit && canCamp)
            {
                if (!HasActionName(sd, "就地歇息"))
                {
                    sd.Actions.Add(new SceneAction { ActionName = "就地歇息", ThresholdType = ThresholdType.None, ThresholdValue = 0, EffectType = ActionEffectType.Camp });
                }
            }

            // Append active event options (non-blocking: standard actions remain available)
            if (EventManager.CurrentActiveEvent != null)
            {
                foreach (var option in EventManager.CurrentActiveEvent.Options)
                {
                    if (!HasActionName(sd, option.OptionText))
                    {
                        sd.Actions.Add(new SceneAction
                        {
                            ActionName = option.OptionText,
                            ThresholdType = option.ThresholdType,
                            ThresholdValue = option.ThresholdValue,
                            RequiredItem = option.RequiredCardId != CardId.None ? option.RequiredCardId.ToString() : "",
                            EffectType = option.EffectType
                        });
                    }
                }
            }

            // Pliers unequip check
            Deck deck = GameState.Instance.DeckInstance;
            if (CardQueryHelper.HasCardEquipped(deck, CardId.KeyJerryCollar) && CardQueryHelper.HasCardAnywhere(deck, CardId.KeyPliers))
            {
                if (!HasActionName(sd, "使用鐵鉗剪斷項圈"))
                {
                    sd.Actions.Add(new SceneAction
                    {
                        ActionName = "使用鐵鉗剪斷項圈",
                        ThresholdType = ThresholdType.None,
                        ThresholdValue = 0,
                        EffectType = ActionEffectType.UsePliersToRemoveCollar
                    });
                }
            }

            // Leo craft check
            if (GameState.Instance.PlayerInstance?.CharacterData?.CharacterId == DeepForest.Character.CharacterId.Leo)
            {
                bool hasCampOption = HasActionName(sd, "紮營") || HasActionName(sd, "就地歇息") || sd.Decals.Contains("campfire");
                if (hasCampOption)
                {
                    if (CardQueryHelper.HasCardAnywhere(deck, CardId.ConsumableRepellent) && 
                        CardQueryHelper.HasCardAnywhere(deck, CardId.EquipmentLighter) &&
                        !CardQueryHelper.HasCardAnywhere(deck, CardId.EquipmentFlamethrower))
                    {
                        if (!HasActionName(sd, "手工"))
                        {
                            sd.Actions.Add(new SceneAction
                            {
                                ActionName = "手工",
                                ThresholdType = ThresholdType.None,
                                ThresholdValue = 0,
                                EffectType = ActionEffectType.LeoCraft
                            });
                        }
                    }
                }
            }
        }

        private static void AddTerrainActions(SceneData sd, string terrain, string side)
        {
            bool canCamp = !GameState.Instance.IsDescentActive || sd.Decals.Contains("campfire");
            switch (terrain)
            {
                case "riverside":
                    if (!HasActionName(sd, "捕魚"))
                        sd.Actions.Add(new SceneAction { ActionName = "捕魚", ThresholdType = ThresholdType.Dex, ThresholdValue = 3, EffectType = ActionEffectType.Fish });
                    if (!HasActionName(sd, "裝水"))
                        sd.Actions.Add(new SceneAction { ActionName = "裝水", RequiredItem = "水瓶", EffectType = ActionEffectType.CollectWater });
                    break;
                case "cabin":
                    if (canCamp && !HasActionName(sd, "紮營"))
                        sd.Actions.Add(new SceneAction { ActionName = "紮營", ThresholdType = ThresholdType.Str, ThresholdValue = 2, EffectType = ActionEffectType.Camp });
                    break;
                case "ruins":
                    if (canCamp && !HasActionName(sd, "紮營"))
                        sd.Actions.Add(new SceneAction { ActionName = "紮營", ThresholdType = ThresholdType.Str, ThresholdValue = 2, EffectType = ActionEffectType.Camp });
                    if (!HasActionName(sd, $"清理{side}塌方通道"))
                        sd.Actions.Add(new SceneAction { ActionName = $"清理{side}塌方通道", ThresholdType = ThresholdType.Str, ThresholdValue = 4, EffectType = ActionEffectType.ClearRuinsPassage });
                    if (!HasActionName(sd, $"搜尋{side}遺跡入口"))
                        sd.Actions.Add(new SceneAction { ActionName = $"搜尋{side}遺跡入口", ThresholdType = ThresholdType.Wis, ThresholdValue = 3, EffectType = ActionEffectType.SearchRuinsEntrance });
                    break;
                case "woodland":
                case "swamp":
                    string envName = terrain == "woodland" ? "樹林" : "沼澤";
                    if (!HasActionName(sd, $"探索{side}{envName}"))
                        sd.Actions.Add(new SceneAction { ActionName = $"探索{side}{envName}", ThresholdType = ThresholdType.Wis, ThresholdValue = 2, EffectType = ActionEffectType.Search });
                    break;
            }
        }

        private static bool IsTerrainPassable(string terrain, out int wisThreshold, out int dexThreshold)
        {
            wisThreshold = 0;
            dexThreshold = 0;
            if (terrain == "swamp")
            {
                dexThreshold = 3;
                return true;
            }
            if (terrain == "ruins")
            {
                wisThreshold = 2;
                return true;
            }
            return false;
        }

        private static bool HasActionName(SceneData sd, string actionName)
        {
            return sd.Actions.Any(action => action.ActionName == actionName);
        }

        public static void RemoveActionFromCurrentScene(string prefix)
        {
            var mapManager = MapManager.Instance;
            var state = Core.GameState.Instance;
            var actions = (state.IsIndoor && mapManager.CurrentIndoorScene != null)
                ? mapManager.CurrentIndoorScene.Actions
                : mapManager.Nodes[mapManager.CurrentNodeId].SceneData.Actions;

            for (int i = actions.Count - 1; i >= 0; i--)
            {
                if (actions[i].ActionName.StartsWith(prefix))
                {
                    actions.RemoveAt(i);
                }
            }
        }
    }
}
