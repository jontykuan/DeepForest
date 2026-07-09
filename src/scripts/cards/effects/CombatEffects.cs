using DeepForest.Core;
using DeepForest.Scene;
using DeepForest.Combat;

namespace DeepForest.Cards.Effects
{
    [ActionEffect(ActionEffectType.CombatClash)]
    public class CombatClashEffect : IActionEffect
    {
        public bool CanExecute(ActionContext context) => true;
        public ActionResult Execute(ActionContext context)
        {
            CombatManager.Instance.ResolveClash();
            return new ActionResult { Success = true, LogMessage = "對決衝突結算" };
        }
    }
}
