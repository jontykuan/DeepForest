using Godot;
using System;
using DeepForest.Core;
using DeepForest.Character;
using DeepForest.Cards;

namespace DeepForest.Narrative;

public enum EndingType
{
    None,
    Escape,
    Disappearance,
    BrutalityEnding,
    CorruptionEnding,
    EvilEnding
}

public partial class EndingManager : Node
{
    public static EndingManager Instance { get; private set; } = null!;

    [Signal] public delegate void EndingTriggeredEventHandler(int endingType, string title, string description);

    public override void _Ready()
    {
        Instance = this;
    }

    public bool CheckEndGameConditions()
    {
        Player player = GameState.Instance.PlayerInstance;
        Deck deck = GameState.Instance.DeckInstance;

        if (player.Brutality >= 80)
        {
            TriggerEnding(EndingType.BrutalityEnding, "暴戾結局：林間捕食者", 
                "你徹底被暴力支配，不再尋求逃離。你融為森林的一部分，成為在黑暗中巡狩、撕裂一切生靈的怪物。");
            return true;
        }

        if (player.Corruption >= 80)
        {
            TriggerEnding(EndingType.CorruptionEnding, "邪祟結局：不可名狀的歸宿", 
                "你的雙眼已被漆黑的低語覆蓋。虛空中的繁星歸位，你高高舉起雙臂，心甘情願地走入那不可名狀的深淵本體之中。");
            return true;
        }

        if (player.Evil >= 80)
        {
            TriggerEnding(EndingType.EvilEnding, "刑偵結局：鐵窗後的真相", 
                "你終於逃出了森林，但身上卻沾滿了無辜者的鮮血。調查官們在森林邊緣逮捕了你。在審訊室的冷光下，你緩緩供述出罪行。");
            return true;
        }

        if (player.CurrentHp <= 0)
        {
            TriggerEnding(EndingType.Disappearance, "人間蒸發：力竭身亡", 
                "你的視線逐漸模糊，體力耗盡的肉身倒在潮濕的泥土中。森林的藤蔓緩慢攀上你的四肢，幾天後，沒有人能再找到你的痕跡。");
            return true;
        }

        if (player.CurrentSanity <= 0)
        {
            TriggerEnding(EndingType.Disappearance, "人間蒸發：狂亂消逝", 
                "理智的防線完全崩潰。你抱著頭尖叫著衝入幽暗的密林深處，再也沒有回來。");
            return true;
        }

        if (GameState.Instance.CurrentDay >= 20)
        {
            TriggerEnding(EndingType.Disappearance, "人間蒸發：迷失長夜", 
                "你已經在森林裡被困了太久。森林的結構每天都在重組，你最終徹底迷失在無限延伸的迷霧迷宮中。");
            return true;
        }

        bool hasMap = StatusEffect.HasEffect(deck.Hand, "地圖殘片") || 
                      StatusEffect.HasEffect(deck.DiscardPile, "地圖殘片") ||
                      StatusEffect.HasEffect(deck.DrawPile, "地圖殘片") ||
                      StatusEffect.HasEffect(deck.EquippedCards, "地圖殘片");

        if (hasMap && GameState.Instance.CurrentDepth >= 30)
        {
            TriggerEnding(EndingType.Escape, "順利逃出：破霧重圓", 
                "對照著拼湊完成的地圖，你穿過了最後一片迷霧，重新看見了人類世界的曙光。你逃出來了。");
            return true;
        }

        return false;
    }

    private void TriggerEnding(EndingType type, string title, string description)
    {
        GameState.Instance.AddLog($"【結局觸發】{title}");
        EmitSignal(SignalName.EndingTriggered, (int)type, title, description);
        
        StoryUnlock.Instance?.UnlockStorySegment(title, description);
    }
}
