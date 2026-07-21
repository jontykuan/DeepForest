using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DeepForest.Cards;
using DeepForest.Core;
using DeepForest.Scene;
using ColorPalette = DeepForest.Core.ColorPalette;

namespace DeepForest.UI
{
    public class ActionPanel
    {
        private readonly VBoxContainer _actionList;
        public List<SceneAction> ActiveActions { get; private set; } = new();

        public ActionPanel(VBoxContainer actionList)
        {
            _actionList = actionList;
        }

        public void UpdateActions(Action<SceneAction> onActionSelected)
        {
            ActiveActions.Clear();
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

            var cp = ColorPalette.Instance;

            RichTextLabel descLabel = new RichTextLabel();
            descLabel.BbcodeEnabled = true;
            descLabel.FitContent = true;
            descLabel.ScrollActive = false;
            descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            descLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

            string titleHex = cp.SilentWhite.ToHtml(false);
            string descHex = cp.PaleGray.ToHtml(false);
            string greenHex = cp.RadiantGreen.ToHtml(false);

            descLabel.Text = $"[color=#{titleHex}]{sceneName}[/color]\n[color=#{descHex}]{sceneDescription}[/color]\n\n[color=#{greenHex}][ 可執行行動 ]：[/color]";
            _actionList.AddChild(descLabel);

            foreach (var action in activeActions)
            {
                HBoxContainer row = new HBoxContainer();
                Button actionButton = new Button();
                
                var (prefix, actionName) = GetActionLabelParts(action);
                string paddedPrefix = string.IsNullOrEmpty(prefix) ? "" : PadPrefix(prefix, 30);
                string arrow = string.IsNullOrEmpty(prefix) ? "" : " ▶ ";
                string baseText = $"{paddedPrefix}{arrow}{actionName}";
                
                actionButton.Text = "  " + baseText;
                
                actionButton.MouseEntered += () => {
                    if (!actionButton.Disabled) actionButton.Text = "» " + baseText;
                };
                actionButton.MouseExited += () => {
                    actionButton.Text = "  " + baseText;
                };
                actionButton.FocusEntered += () => {
                    if (!actionButton.Disabled) actionButton.Text = "» " + baseText;
                };
                actionButton.FocusExited += () => {
                    actionButton.Text = "  " + baseText;
                };
                
                bool available = IsActionAvailable(action, out string state);
                
                if (state == "Available" || state == "ForceAvailable")
                {
                    actionButton.Modulate = cp.RadiantGreen; 
                }
                else if (state == "InsufficientPoints")
                {
                    actionButton.Modulate = cp.GrayGreen; 
                    actionButton.Disabled = true;
                }
                else 
                {
                    actionButton.Modulate = cp.BloodRed; 
                    actionButton.Disabled = true;
                }

                actionButton.Pressed += () => onActionSelected(action);
                row.AddChild(actionButton);
                _actionList.AddChild(row);
            }
            ActiveActions = activeActions;
        }

        private int GetVisualWidth(string s)
        {
            int width = 0;
            foreach (char c in s)
            {
                width += (c > 255) ? 2 : 1;
            }
            return width;
        }

        private string PadPrefix(string prefix, int targetWidth)
        {
            int currentWidth = GetVisualWidth(prefix);
            if (currentWidth >= targetWidth) return prefix;
            return prefix + new string(' ', targetWidth - currentWidth);
        }

        private (string prefix, string label) GetActionLabelParts(SceneAction action)
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
                ThresholdType.StrOrWis => $"[力量/智慧 {Math.Max(TurnManager.Instance.AccumulatedStr, TurnManager.Instance.AccumulatedWis)}/{action.ThresholdValue}]",
                _ => ""
            };

            int currentVal = action.ThresholdType switch
            {
                ThresholdType.Str => TurnManager.Instance.AccumulatedStr,
                ThresholdType.Dex => TurnManager.Instance.AccumulatedDex,
                ThresholdType.Wis => TurnManager.Instance.AccumulatedWis,
                ThresholdType.Any => TurnManager.Instance.AccumulatedStr + TurnManager.Instance.AccumulatedDex + TurnManager.Instance.AccumulatedWis,
                ThresholdType.StrOrWis => Math.Max(TurnManager.Instance.AccumulatedStr, TurnManager.Instance.AccumulatedWis),
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

            return (prefix, action.ActionName);
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
                if (action.RequiredItem == "容器" || action.RequiredItem == "container")
                {
                    hasItem = CardQueryHelper.HasCardAnywhere(GameState.Instance.DeckInstance, CardEffectTag.Container);
                }
                else if (Enum.TryParse<CardId>(action.RequiredItem, out var reqCardId))
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
                ThresholdType.StrOrWis => Math.Max(TurnManager.Instance.AccumulatedStr, TurnManager.Instance.AccumulatedWis),
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
