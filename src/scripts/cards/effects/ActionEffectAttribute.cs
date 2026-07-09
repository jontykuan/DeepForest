using System;
using DeepForest.Scene;

namespace DeepForest.Cards.Effects
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ActionEffectAttribute : Attribute
    {
        public ActionEffectType EffectType { get; }
        public ActionEffectAttribute(ActionEffectType effectType)
        {
            EffectType = effectType;
        }
    }
}
