using Godot;
using System.Collections.Generic;
using System.Linq;
using DeepForest.Cards;

namespace DeepForest.Character;

public static class StatusEffect
{
    public static bool HasEffect(IEnumerable<Card> cards, CardEffectTag tag)
    {
        return cards.Any(card => card.EffectTags.HasFlag(tag));
    }

    public static bool HasLeftEyeBlindness(IEnumerable<Card> cards) => HasEffect(cards, CardEffectTag.BlindnessLeft);
    public static bool HasRightEyeBlindness(IEnumerable<Card> cards) => HasEffect(cards, CardEffectTag.BlindnessRight);
    public static bool HasBrokenFinger(IEnumerable<Card> cards) => HasEffect(cards, CardEffectTag.BrokenFinger);
    public static bool HasBrokenArm(IEnumerable<Card> cards) => HasEffect(cards, CardEffectTag.BrokenArm);
    public static bool HasTextDistortion(IEnumerable<Card> cards) => HasEffect(cards, CardEffectTag.TextDistortion) || HasEffect(cards, CardEffectTag.Hallucination);

    public static int GetFractureCount(IEnumerable<Card> cards)
    {
        return cards.Count(card => card.EffectTags.HasFlag(CardEffectTag.Fracture));
    }
}
