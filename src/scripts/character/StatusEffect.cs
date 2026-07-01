using Godot;
using System.Collections.Generic;
using DeepForest.Cards;

namespace DeepForest.Character;

public static class StatusEffect
{
    public static bool HasEffect(IEnumerable<Card> cards, string effectName)
    {
        foreach (var card in cards)
        {
            if (card.CardName == effectName)
                return true;
        }
        return false;
    }

    public static bool HasLeftEyeBlindness(IEnumerable<Card> cards) => HasEffect(cards, "失明（左眼）");
    public static bool HasRightEyeBlindness(IEnumerable<Card> cards) => HasEffect(cards, "失明（右眼）");
    public static bool HasBrokenFinger(IEnumerable<Card> cards) => HasEffect(cards, "手指骨折");
    public static bool HasBrokenArm(IEnumerable<Card> cards) => HasEffect(cards, "手臂脫臼");
    public static bool HasTextDistortion(IEnumerable<Card> cards) => HasEffect(cards, "文字扭曲") || HasEffect(cards, "幻聽");

    public static int GetFractureCount(IEnumerable<Card> cards)
    {
        int count = 0;
        foreach (var card in cards)
        {
            if (card.CardName == "骨折")
                count++;
        }
        return count;
    }
}
