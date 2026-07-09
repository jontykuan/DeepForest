using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DeepForest.Scene;
using DeepForest.Narrative;

namespace DeepForest.Cards.Effects
{
    public class ActionResolver
    {
        private static ActionResolver? _instance;
        public static ActionResolver Instance => _instance ??= new ActionResolver();

        private readonly Dictionary<ActionEffectType, IActionEffect> _effects = new();

        public ActionResolver()
        {
            var effectTypes = typeof(IActionEffect).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract 
                    && typeof(IActionEffect).IsAssignableFrom(t));
                    
            foreach (var type in effectTypes)
            {
                var attrs = type.GetCustomAttributes<ActionEffectAttribute>();
                if (attrs != null && attrs.Any())
                {
                    var instance = (IActionEffect)Activator.CreateInstance(type)!;
                    foreach (var attr in attrs)
                    {
                        _effects[attr.EffectType] = instance;
                    }
                }
            }
        }

        public ActionResult Resolve(SceneAction action, ActionContext context)
        {
            ActionResult result = null;

            if (EventManager.CurrentActiveEvent != null)
            {
                var option = EventManager.CurrentActiveEvent.Options
                    .FirstOrDefault(o => o.OptionText == action.ActionName);
                if (option != null)
                {
                    string logMessage = EventManager.ResolveEventOption(option, context.Player, context.Deck);
                    
                    if (option.EffectType != ActionEffectType.None && option.EffectType != ActionEffectType.CombatClash)
                    {
                        if (_effects.TryGetValue(option.EffectType, out var effect))
                        {
                            var innerResult = effect.Execute(context);
                            if (!innerResult.Success)
                            {
                                return innerResult;
                            }
                            logMessage += "\n" + innerResult.LogMessage;
                        }
                    }

                    if (option.EffectType == ActionEffectType.CombatClash)
                    {
                        if (_effects.TryGetValue(ActionEffectType.CombatClash, out var combatEffect))
                        {
                            var innerResult = combatEffect.Execute(context);
                            logMessage += "\n" + innerResult.LogMessage;
                        }
                    }

                    result = new ActionResult { Success = true, LogMessage = logMessage };
                }
            }

            if (result == null && _effects.TryGetValue(action.EffectType, out var standardEffect))
            {
                if (!standardEffect.CanExecute(context))
                    return new ActionResult { Success = false, LogMessage = "條件不足，無法執行。" };
                var innerResult = standardEffect.Execute(context);
                if (innerResult.Success && action.TargetNodeId >= 0 && MapManager.Instance != null)
                {
                    MapManager.Instance.CurrentNodeId = action.TargetNodeId;
                }
                result = innerResult;
            }

            if (result == null && action.TargetNodeId >= 0 && MapManager.Instance != null)
            {
                MapManager.Instance.CurrentNodeId = action.TargetNodeId;
                result = new ActionResult { Success = true, LogMessage = "已移至指定區域。" };
            }

            if (result == null)
            {
                result = new ActionResult { Success = false, LogMessage = $"未知的行動效果: {action.EffectType}" };
            }

            if (result.Success && CardQueryHelper.HasCardEquipped(context.Deck, CardId.EquipmentPoliceBaton))
            {
                context.Player.Brutality = Math.Min(100, context.Player.Brutality + 1);
                result = new ActionResult
                {
                    Success = result.Success,
                    LogMessage = result.LogMessage + "\n【警棍副作用】你的暴力傾向微幅增加。（暴戾 +1）",
                    RemoveActionAfterUse = result.RemoveActionAfterUse,
                    TriggerSceneChange = result.TriggerSceneChange,
                    NewScene = result.NewScene
                };
            }

            return result;
        }

        public bool CanResolve(SceneAction action, ActionContext context)
        {
            if (_effects.TryGetValue(action.EffectType, out var effect))
            {
                return effect.CanExecute(context);
            }
            return false;
        }
    }
}
