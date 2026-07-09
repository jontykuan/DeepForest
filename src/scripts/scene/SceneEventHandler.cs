using Godot;
using System;
using DeepForest.Core;
using DeepForest.Cards;
using DeepForest.Combat;
using DeepForest.Narrative;

namespace DeepForest.Scene
{
    public static class SceneEventHandler
    {
        public static void OnPlayerEnterNode(int nodeId)
        {
            if (nodeId == 0) return;
            var mapManager = MapManager.Instance;
            if (!mapManager.Nodes.TryGetValue(nodeId, out var node)) return;

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



            // 3. 資料驅動事件系統：掃描並觸發符合條件的事件
            EventManager.CheckAndTriggerEvent(node);

            // 重新生成行動（包含事件選項）
            ActionGenerator.GenerateDynamicActions(sd, nodeId);

            // 3. 檢查是否有戰鬥貼圖並自動觸發戰鬥
            foreach (var decal in sd.Decals)
            {
                if (decal.StartsWith("combat_wolf"))
                {
                    var enemyData = EnemyFactory.GetEnemyDataForDecal("combat_wolf");
                    CombatManager.Instance.StartCombat(enemyData);
                    break;
                }
                else if (decal.StartsWith("combat_ghost"))
                {
                    var enemyData = EnemyFactory.GetEnemyDataForDecal("combat_ghost");
                    CombatManager.Instance.StartCombat(enemyData);
                    break;
                }
                else if (decal.StartsWith("combat_cultist"))
                {
                    var enemyData = EnemyFactory.GetEnemyDataForDecal("combat_cultist");
                    CombatManager.Instance.StartCombat(enemyData);
                    break;
                }
                else if (decal.StartsWith("combat_sarah"))
                {
                    var enemyData = EnemyFactory.GetEnemyDataForDecal("combat_sarah");
                    CombatManager.Instance.StartCombat(enemyData);
                    break;
                }
            }
        }
    }
}
