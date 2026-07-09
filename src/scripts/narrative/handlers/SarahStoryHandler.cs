using System;
using DeepForest.Cards;
using DeepForest.Character;
using DeepForest.Core;

namespace DeepForest.Narrative.Handlers
{
    public class SarahStoryHandler : ICharacterStoryHandler
    {
        public CharacterId CharacterId => CharacterId.Sarah;
        public CharacterStoryFlags Flags { get; } = new();

        public EndingResult? CheckHiddenStatEndings(Player player, Deck deck)
        {
            if (player.Brutality >= 90)
            {
                return new EndingResult("BrutalityEnding", "狂躁的主婦",
                    "家庭的重壓與森林的險惡徹底摧毀了你的理智。你開始用暴力支配一切，揮舞著柴刀將任何靠近的身影視為獵物。");
            }
            if (player.Corruption >= 90)
            {
                return new EndingResult("CorruptionEnding", "林間棄子",
                    "在不可名狀的林間低語中，你心中的怨恨無限放大。你將這一切不幸歸咎於懷中啼哭的稚子，最終在冷漠中將他拋棄在無邊的濃霧深處...");
            }
            if (player.Evil >= 90)
            {
                return new EndingResult("EvilEnding", "虛偽的倖存者",
                    "你雖然走出了迷霧，但一路上為了苟活而編造的謊言、背叛同伴的惡行終究被揭露。你被社會孤立，在眾人的指責與內疚中苟延殘喘。");
            }
            return null;
        }

        public EndingResult? CheckEscapeEndings(Player player, Deck deck)
        {
            bool hasDivorce = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyDivorceAgreement);
            bool hasLoyalty = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyChastityLie);
            bool hasTape = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRecordingTape);
            bool hasTommy = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyBabySon);

            if (hasDivorce && hasLoyalty && hasTape)
            {
                if (hasTommy)
                {
                    return new EndingResult("Escape", "成就：重獲新生",
                        "你成功帶著兒子、離婚協議書、貞潔謊言以及錄音帶逃出了這片森林。你徹底擺脫了過去的噩夢，迎來了真正的新生！");
                }
                else
                {
                    return new EndingResult("Escape", "成就：解脫",
                        "你成功擺脫了糾纏與謊言，拿到了離婚協議書與錄音帶逃出森林。雖然你失去了兒子，但你獲得了個人的解脫，回到了自由的陽光下。");
                }
            }
            return null;
        }

        public void UnlockBackgroundStories(Player player, Deck deck)
        {
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyDivorceAgreement))
                StoryUnlock.Instance?.UnlockStorySegment("劉淑莉背景：決心的代價", "解鎖了劉淑莉在家庭冷暴力下苦苦支撐，最終決心離婚的心路歷程。");
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyChastityLie))
                StoryUnlock.Instance?.UnlockStorySegment("劉淑莉背景：不可告人的秘密", "解鎖了劉淑莉為了家庭與尊嚴，不得不編造的貞潔謊言與過往。");
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRecordingTape))
                StoryUnlock.Instance?.UnlockStorySegment("劉淑莉背景：絕望的錄音", "解鎖了劉淑莉在收音機和錄音帶中發現丈夫罪證時的震驚與絕望。");
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyBabySon))
                StoryUnlock.Instance?.UnlockStorySegment("劉淑莉背景：失而復得的執念", "解鎖了劉淑莉對兒子湯明亮的病態執念，以及在迷霧中尋子的瘋狂。");
        }

        public bool HandleNpcEncounter(Player player, Deck deck, CharacterId npcId, int choice)
        {
            if (npcId == CharacterId.Celin)
            {
                int playerWis = TurnManager.Instance.AccumulatedWis;
                if (playerWis >= 5)
                {
                    Card tape = CardFactory.CreateCard(CardId.KeyRecordingTape);
                    if (tape != null) deck.AddCardToDiscardPile(tape);
                    GameState.Instance.AddLog("【遭遇 Celin】你以高智慧看穿了李曉琳的恍惚，從她隨身物品中獲得了【錄音帶】！");
                }
                else
                {
                    player.CurrentSanity = Math.Max(0, player.CurrentSanity - 10);
                    GameState.Instance.AddLog("【遭遇 Celin】李曉琳胡言亂語，你的心智受到干擾（理智 -10）。");
                }
                return true;
            }

            if (npcId == CharacterId.Leo)
            {
                if (choice == 1) // 出賣肉體
                {
                    Card lies = CardFactory.CreateCard(CardId.KeyChastityLie);
                    if (lies != null) deck.AddCardToDiscardPile(lies);
                    player.Evil = Math.Min(100, player.Evil + 20);
                    GameState.Instance.AddLog("【出賣肉體】你選擇出賣身體與 Leo 妥協，獲得了「貞潔謊言」！（邪惡 +20）");
                }
                else if (choice == 2) // 暴力威脅
                {
                    Card lies = CardFactory.CreateCard(CardId.KeyChastityLie);
                    if (lies != null) deck.AddCardToDiscardPile(lies);
                    player.Brutality = Math.Min(100, player.Brutality + 20);
                    GameState.Instance.AddLog("【暴力威脅】你用暴力威脅 Leo，獲得了「貞潔謊言」！（暴戾 +20）");
                }
                else if (choice == 3) // 說服 (WIS >= 5 test)
                {
                    int playerWis = TurnManager.Instance.AccumulatedWis;
                    if (playerWis >= 5)
                    {
                        Card lies = CardFactory.CreateCard(CardId.KeyChastityLie);
                        if (lies != null) deck.AddCardToDiscardPile(lies);
                        GameState.Instance.AddLog("【智慧說服】你用智慧看穿了 Leo 的謊言，說服他交出了「貞潔謊言」！");
                    }
                    else
                    {
                        player.CurrentSanity = Math.Max(0, player.CurrentSanity - 10);
                        GameState.Instance.AddLog("【說服失敗】你的智慧不足以戳穿 Leo 的謊言，心智受到打擊（理智 -10）。");
                    }
                }
                return true;
            }
            return false;
        }

        public bool HandleSpecialEvent(Player player, Deck deck, string eventId, int choice)
        {
            if (eventId == "rusty_iron_box")
            {
                if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyOldKey))
                {
                    Card tape = CardFactory.CreateCard(CardId.KeyRecordingTape);
                    if (tape != null)
                    {
                        deck.AddCardToDiscardPile(tape);
                        GameState.Instance.AddLog("【密室探索】你用老舊鑰匙打開了生鏽鐵盒，在其中找到了一盒舊的【錄音帶】！");
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
