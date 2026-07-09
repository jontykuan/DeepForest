using System;
using DeepForest.Cards;
using DeepForest.Character;
using DeepForest.Core;

namespace DeepForest.Narrative.Handlers
{
    public class JohnStoryHandler : ICharacterStoryHandler
    {
        public CharacterId CharacterId => CharacterId.John;
        public CharacterStoryFlags Flags { get; } = new();

        public EndingResult? CheckHiddenStatEndings(Player player, Deck deck)
        {
            if (player.Brutality >= 90)
            {
                return new EndingResult("BrutalityEnding", "林間捕食者",
                    "力竭的本能驅使你退化為純粹的林間野獸，你將以生肉與鮮血為食，永遠徘徊在長夜之中。");
            }
            if (player.Corruption >= 90)
            {
                return new EndingResult("CorruptionEnding", "林間弒妻",
                    "在林間無孔不入的邪祟蠱惑下，你心中的猜忌與暴戾被無限放大，最終你在狂亂中用柴刀手刃了前來尋你的妻子...");
            }
            if (player.Evil >= 90)
            {
                return new EndingResult("EvilEnding", "鐵窗後的真相",
                    "你雖走出了迷霧，但早已在暗處被警方包圍。你因蓄意侵占公款、偽造本票及涉嫌多起地下社團非法勾當而被逮捕，後半生將在鐵窗後度過。");
            }
            return null;
        }

        public EndingResult? CheckEscapeEndings(Player player, Deck deck)
        {
            bool hasDivorce = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyDivorceAgreement);
            bool hasTape = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRecordingTape);
            bool hasNote = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyPromissoryNote);

            if (hasDivorce && hasTape && hasNote)
            {
                return new EndingResult("Escape", "成就：隱而未發",
                    "帶著警方的錄音帶與犯罪本票，你與妻子的離婚協議書已簽字，你成功走出了森林。這場糾紛與危機被隱藏了起來，未來的衝突依然暗流湧動...");
            }
            return null;
        }

        public void UnlockBackgroundStories(Player player, Deck deck)
        {
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyDivorceAgreement))
                StoryUnlock.Instance?.UnlockStorySegment("湯自強背景：婚姻危機的起因", "解鎖了湯自強因為出軌與家庭暴力導致婚姻徹底破裂的前塵往事。");
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRecordingTape))
                StoryUnlock.Instance?.UnlockStorySegment("湯自強背景：犯罪的邊緣", "解鎖了湯自強在利益與誘惑面前，逐漸走向犯罪深淵的秘密錄音。");
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyPromissoryNote))
                StoryUnlock.Instance?.UnlockStorySegment("湯自強背景：債務纏身", "解鎖了湯自強私下簽署的高額本票，背負著無盡高利貸利息的絕望深淵。");
        }

        public bool HandleNpcEncounter(Player player, Deck deck, CharacterId npcId, int choice)
        {
            if (npcId == CharacterId.Sarah)
            {
                if (choice == 1) // 用足跡換信任 (wis)
                {
                    if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyTinyFootprints))
                    {
                        CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeyTinyFootprints);
                        Card divorce = CardFactory.CreateCard(CardId.KeyDivorceAgreement);
                        if (divorce != null) deck.AddCardToDiscardPile(divorce);
                        player.Evil += 10;
                        GameState.Instance.AddLog("【交換籌碼】你用小小的足跡換取了 Sarah 的離婚協議書，解解除婚姻危機，但心中多了一份愧疚（邪惡 +10）。");
                    }
                    else
                    {
                        GameState.Instance.AddLog("你試圖用足跡換取離婚協議書，但你身上並沒有足跡線索。");
                    }
                }
                else if (choice == 2) // 暴力威脅
                {
                    Card divorce = CardFactory.CreateCard(CardId.KeyDivorceAgreement);
                    if (divorce != null) deck.AddCardToDiscardPile(divorce);
                    player.Brutality += 20;
                    player.Evil += 10;
                    GameState.Instance.AddLog("【暴力威脅】你動用暴力強行搶奪了 Sarah 攜帶的離婚協議書（暴戾 +20，邪惡 +10）！");
                }
                return true;
            }
            else if (npcId == CharacterId.Nancy)
            {
                if (choice == 1) // 暴力脅迫 (STR >= 5)
                {
                    int playerStr = TurnManager.Instance.AccumulatedStr;
                    if (playerStr >= 5)
                    {
                        Card tape = CardFactory.CreateCard(CardId.KeyRecordingTape);
                        if (tape != null) deck.AddCardToDiscardPile(tape);
                        GameState.Instance.AddLog("【暴力威脅】你憑藉強大的力量威脅 Nancy，強行拿走了她身上的「錄音帶」！");
                    }
                    else
                    {
                        player.CurrentHp = Math.Max(1, player.CurrentHp - 10);
                        GameState.Instance.AddLog("【威脅失敗】你試圖威脅 Nancy，但力量不足，Nancy 拼死抵抗並抓傷了你（體力 -10）。");
                    }
                }
                else if (choice == 2) // 用偷拍照片交換
                {
                    if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeySneakPhoto))
                    {
                        CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeySneakPhoto);
                        Card tape = CardFactory.CreateCard(CardId.KeyRecordingTape);
                        if (tape != null) deck.AddCardToDiscardPile(tape);
                        GameState.Instance.AddLog("【交易把柄】你交出了「偷拍照片」，作為交換，Nancy 把「錄音帶」還給了你。");
                    }
                    else
                    {
                        GameState.Instance.AddLog("你試圖用照片交換錄音帶，但你身上並沒有「偷拍照片」。");
                    }
                }
                return true;
            }
            else if (npcId == CharacterId.Celin)
            {
                if (choice == 1) // 力量門檻進入戰鬥/判定 (STR >= 5)
                {
                    int playerStr = TurnManager.Instance.AccumulatedStr;
                    if (playerStr >= 5)
                    {
                        Card photo = CardFactory.CreateCard(CardId.KeySneakPhoto);
                        if (photo != null) deck.AddCardToDiscardPile(photo);
                        GameState.Instance.AddLog("【武力奪取】你憑藉力量震懾了 Celin，從她身上奪得了「偷拍照片」！");
                    }
                    else
                    {
                        player.CurrentHp = Math.Max(1, player.CurrentHp - 10);
                        GameState.Instance.AddLog("【奪取失敗】你的力量不足以震懾 Celin，反被她劃傷（體力 -10）。");
                    }
                }
                else if (choice == 2) // 智慧門檻說服 (WIS >= 5)
                {
                    int playerWis = TurnManager.Instance.AccumulatedWis;
                    if (playerWis >= 5)
                    {
                        Card photo = CardFactory.CreateCard(CardId.KeySneakPhoto);
                        if (photo != null) deck.AddCardToDiscardPile(photo);
                        GameState.Instance.AddLog("【心機說服】你用言語說服了 Celin，智取了「偷拍照片」！");
                    }
                    else
                    {
                        player.CurrentSanity = Math.Max(0, player.CurrentSanity - 15);
                        GameState.Instance.AddLog("【說服失敗】你的智慧沒能看透 Celin，反被她的病態囈語干擾（理智 -15）。");
                    }
                }
                return true;
            }
            return false;
        }

        public bool HandleSpecialEvent(Player player, Deck deck, string eventId, int choice)
        {
            if (eventId == "wolf_victory")
            {
                if (player.Brutality < 40)
                {
                    int playerWis = TurnManager.Instance.AccumulatedWis;
                    if (playerWis >= 3)
                    {
                        Card footprint = CardFactory.CreateCard(CardId.KeyTinyFootprints);
                        if (footprint != null) deck.AddCardToDiscardPile(footprint);
                        GameState.Instance.AddLog("【尋子線索】你以過人的智慧，在狼群印記中識別出了「小小的足跡」！");
                    }
                    else
                    {
                        GameState.Instance.AddLog("你的智慧不足以在複雜的環境中分辨出細微的足跡。");
                    }
                }
                else
                {
                    int playerDex = TurnManager.Instance.AccumulatedDex;
                    if (playerDex >= 3)
                    {
                        Card footprint = CardFactory.CreateCard(CardId.KeyTinyFootprints);
                        if (footprint != null) deck.AddCardToDiscardPile(footprint);
                        player.CurrentHp = System.Math.Max(1, player.CurrentHp - 10);
                        GameState.Instance.AddLog("【強行追蹤】你憑藉靈巧在荊棘中強行搜尋，獲得了「小小的足跡」，但因此被劃傷（體力 -10）。");
                    }
                    else
                    {
                        player.CurrentHp = System.Math.Max(1, player.CurrentHp - 10);
                        GameState.Instance.AddLog("你強行搜尋，受到荊棘劃傷（體力 -10），但沒能發現 any 線索。");
                    }
                }
                return true;
            }
            else if (eventId == "rusty_iron_box")
            {
                if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyOldKey))
                {
                    Card note = CardFactory.CreateCard(CardId.KeyPromissoryNote);
                    if (note != null) deck.AddCardToDiscardPile(note);
                    GameState.Instance.AddLog("【開啟鐵盒】你用老舊鑰匙打開了鐵盒，裡面藏著一張簽了名字的【本票】！");
                    return true;
                }
            }
            return false;
        }
    }
}
