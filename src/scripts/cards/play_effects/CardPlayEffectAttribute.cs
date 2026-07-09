using System;

namespace DeepForest.Cards.Effects
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CardPlayEffectAttribute : Attribute
    {
        public string CardName { get; }
        public CardPlayEffectAttribute(string cardName) => CardName = cardName;
    }
}
