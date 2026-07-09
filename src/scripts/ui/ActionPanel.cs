using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DeepForest.Cards;
using DeepForest.Core;
using DeepForest.Scene;

namespace DeepForest.UI
{
    public class ActionPanel
    {
        private readonly VBoxContainer _actionList;

        public ActionPanel(VBoxContainer actionList)
        {
            _actionList = actionList;
        }

        public void UpdateActions(Action<SceneAction> onActionSelected)
        {
            if (_actionList == null) return;

            foreach (Node child in _actionList.GetChildren())
            {
                child.QueueFree();
            }

            var mapManager = MapManager.Instance;
            var mapNode = mapManager.Nodes[mapManager.CurrentNodeId];
            var sceneData = mapNode.SceneData;

            string sceneName = sceneData.SceneName;
            string sceneDescription = sceneData.SceneDescription;
            List<SceneAction> activeActions = new List<SceneAction>();

            if (GameState.Instance.IsInCombat && GameState.Instance.CurrentEnemy != null)
            {
                var enemy = GameState.Instance.CurrentEnemy;
                string hpStr = enemy.HideHp ? "???/???" : $"{GameState.Instance.CurrentEnemyHp}/{enemy.MaxHp}";
                sceneName = $"【戰鬥】{enemy.EnemyName}";
                sceneDescription = $"一隻【{enemy.EnemyName}】阻擋了你！請從手牌打出屬性卡放入對決區，點擊下方對決進行比拼。";

                if (GameState.Instance.CombatPlayedCards.Count > 0)
                {
                    string playedCardsStr = "\n\n【已投入對決區的卡牌】：";
                    foreach (var c in GameState.Instance.CombatPlayedCards)
                    {
                        playedCardsStr += $"[{c.CardName}] ";
                    }
                    sceneDescription += playedCardsStr;
                }
                else
                {
                    sceneDescription += "\n\n【對決區】：目前空無一物。請打出屬性卡進行比拼。";
                }

                activeActions.Add(new SceneAction { 
                    ActionName = $"揭露結果 ({enemy.EnemyName} {hpStr})", 
                    ThresholdType = ThresholdType.None, 
                    ThresholdValue = 0, 
                    EffectType = ActionEffectType.CombatClash 
                });

                int fleeDex = enemy.IsAggressive ? 5 : 2;
                activeActions.Add(new SceneAction {
                    ActionName = "逃跑並前進",
                    ThresholdType = ThresholdType.Dex,
                    ThresholdValue = fleeDex,
                    EffectType = ActionEffectType.MoveForward
                });
            }
            else if (GameState.Instance.IsIndoor && mapManager.CurrentIndoorScene != null)
            {
                sceneName = mapManager.CurrentIndoorScene.SceneName;
                sceneDescription = mapManager.CurrentIndoorScene.SceneDescription;
                activeActions.AddRange(mapManager.CurrentIndoorScene.Actions);
            }
            else
            {
                activeActions.AddRange(sceneData.Actions);
            }

            Label descLabel = new Label();
            descLabel.Text = $"{sceneName}\n{sceneDescription}\n\n[ 可執行行動 ]：";
            descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            descLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _actionList.AddChild(descLabel);

            foreach (var action in activeActions)
            {
                HBoxContainer row = new HBoxContainer();
                Button actionButton = new Button();
                actionButton.Text = GetActionLabelText(action);
                
                bool available = IsActionAvailable(action, out string state);
                
                if (state == "Available")
                {
                    actionButton.Modulate = new Color(0.22f, 1.0f, 0.08f); 
                }
                else if (state == "InsufficientPoints")
                {
                    actionButton.Modulate = new Color(0.33f, 0.42f, 0.18f); 
                    actionButton.Disabled = true;
                }
                else 
                {
                    actionButton.Modulate = new Color(1.0f, 0.13f, 0.13f); 
                    actionButton.Disabled = true;
                }

                actionButton.Pressed += () => onActionSelected(action);
                row.AddChild(actionButton);
                _actionList.AddChild(row);
            }
        }

        private string GetActionLabelText(SceneAction action)
        {
            int req = action.ThresholdValue;
            if (action.ThresholdType == ThresholdType.Dex)
            {
                if (EnvironmentSystem.Instance != null)
                {
                    req += EnvironmentSystem.Instance.GetDexThresholdModifier();
                }
                if (CardQueryHelper.HasCardEquipped(GameState.Instance.DeckInstance, CardId.EquipmentFlashlight))
                {
                    req = Math.Max(0, req - 1);
                }
            }

            string prefix = action.ThresholdType switch
            {
                ThresholdType.Str => $"[力量 {TurnManager.Instance.AccumulatedStr}/{action.ThresholdValue}]",
                ThresholdType.Dex => $"[靈巧 {TurnManager.Instance.AccumulatedDex}/{req}]",
                ThresholdType.Wis => $"[智慧 {TurnManager.Instance.AccumulatedWis}/{action.ThresholdValue}]",
                ThresholdType.Any => $"[行動 {TurnManager.Instance.AccumulatedStr + TurnManager.Instance.AccumulatedDex + TurnManager.Instance.AccumulatedWis}/{action.ThresholdValue}]",
                _ => ""
            };

            int currentVal = action.ThresholdType switch
            {
                ThresholdType.Str => TurnManager.Instance.AccumulatedStr,
                ThresholdType.Dex => TurnManager.Instance.AccumulatedDex,
                ThresholdType.Wis => TurnManager.Instance.AccumulatedWis,
                ThresholdType.Any => TurnManager.Instance.AccumulatedStr + TurnManager.Instance.AccumulatedDex + TurnManager.Instance.AccumulatedWis,
                _ => 0
            };

            int finalReq = action.ThresholdType == ThresholdType.Dex ? req : action.ThresholdValue;
            if (currentVal < finalReq && CardQueryHelper.HasCardAnywhere(GameState.Instance.DeckInstance, CardId.ActionForce))
            {
                int missing = finalReq - currentVal;
                prefix += $"[強行: 體力 -{missing * 2}]";
            }

            if (!string.IsNullOrEmpty(action.RequiredItem))
            {
                prefix += $"[需要: {action.RequiredItem}]";
            }

            if (action.EffectType == ActionEffectType.LootCorpse)
            {
                prefix += "[理智 -1]";
            }

            return $"{prefix} ▶ {action.ActionName}";
        }

        private bool IsActionAvailable(SceneAction action, out string state)
        {
            if (action.EffectType == ActionEffectType.Camp && GameState.Instance.IsDescentActive)
            {
                string sceneName = "";
                if (MapManager.Instance != null && MapManager.Instance.Nodes.TryGetValue(MapManager.Instance.CurrentNodeId, out var node))
                {
                    sceneName = node.SceneData?.SceneName ?? "";
                }
                
                if (GameState.Instance.IsIndoor && MapManager.Instance?.CurrentIndoorScene != null)
                {
                    sceneName = MapManager.Instance.CurrentIndoorScene.SceneName;
                }

                if (sceneName != "點燃的營火")
                {
                    state = "ConditionsMissing";
                    return false;
                }
            }

            if (action.EffectType == ActionEffectType.LootCorpse)
            {
                if (GameState.Instance.PlayerInstance.CurrentSanity < 1)
                {
                    state = "InsufficientPoints";
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(action.RequiredItem))
            {
                bool hasItem = false;
                if (Enum.TryParse<CardId>(action.RequiredItem, out var reqCardId))
                {
                    hasItem = CardQueryHelper.HasCardAnywhere(GameState.Instance.DeckInstance, reqCardId);
                }
                else
                {
                    hasItem = CardQueryHelper.HasCardAnywhere(GameState.Instance.DeckInstance, action.RequiredItem);
                }

                if (!hasItem)
                {
                    state = "ConditionsMissing";
                    return false;
                }
            }

            int currentVal = action.ThresholdType switch
            {
                ThresholdType.Str => TurnManager.Instance.AccumulatedStr,
                ThresholdType.Dex => TurnManager.Instance.AccumulatedDex,
                ThresholdType.Wis => TurnManager.Instance.AccumulatedWis,
                ThresholdType.Any => TurnManager.Instance.AccumulatedStr + TurnManager.Instance.AccumulatedDex + TurnManager.Instance.AccumulatedWis,
                _ => 0
            };

            int req = action.ThresholdValue;
            if (action.ThresholdType == ThresholdType.Dex)
            {
                if (EnvironmentSystem.Instance != null)
                {
                    req += EnvironmentSystem.Instance.GetDexThresholdModifier();
                }
                if (CardQueryHelper.HasCardEquipped(GameState.Instance.DeckInstance, CardId.EquipmentFlashlight))
                {
                    req = Math.Max(0, req - 1);
                }
            }

            if (currentVal >= req)
            {
                state = "Available";
                return true;
            }
            else
            {
                if (CardQueryHelper.HasCardAnywhere(GameState.Instance.DeckInstance, CardId.ActionForce))
                {
                    int missing = req - currentVal;
                    if (GameState.Instance.PlayerInstance.CurrentHp > missing * 2)
                    {
                        state = "ForceAvailable";
                        return true;
                    }
                }
                state = "InsufficientPoints";
                return false;
            }
        }
    }
}
