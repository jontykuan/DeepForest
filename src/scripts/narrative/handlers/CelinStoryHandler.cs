using System;
using DeepForest.Cards;
using DeepForest.Character;
using DeepForest.Core;

namespace DeepForest.Narrative.Handlers
{
    public class CelinStoryHandler : ICharacterStoryHandler
    {
        public CharacterId CharacterId => CharacterId.Celin;
        public CharacterStoryFlags Flags { get; } = new();

        public EndingResult? CheckHiddenStatEndings(Player player, Deck deck)
        {
            if (player.Brutality >= 90)
            {
                return new EndingResult("BrutalityEnding", "病態的獵犬",
                    "嫉妒與佔有慾將你撕裂。你不再需要任何偽裝，化為狂暴的獵犬，撕碎任何試圖染指哥哥的女人。");
            }
            if (player.Corruption >= 90)
            {
                return new EndingResult("CorruptionEnding", "異形母體",
                    "你完全擁抱了孕育神嗣的禁忌知識。你的肉體在穢祟的重塑下逐漸變異，成為了林間孕育不祥生命的異形母體...");
            }
            if (player.Evil >= 90)
            {
                return new EndingResult("EvilEnding", "瘋狂的操縱者",
                    "你雖然走出了森林，但徹底繼承了哥哥支配他人的邪惡手腕。你用日記與手機中的把柄威脅多名同伴，將她們拉入由你操控的深淵。");
            }
            return null;
        }

        public EndingResult? CheckEscapeEndings(Player player, Deck deck)
        {
            bool hasPhone = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyBrokenPhone);
            bool hasDiary = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyLockedDiary);
            bool hasSeed = CardQueryHelper.HasCardAnywhere(deck, CardId.KeySeedOfLife);
            bool hasWhisper = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyWhisper);

            if (hasPhone && hasDiary && hasSeed)
            {
                return new EndingResult("Escape", "成就：純潔餘孽",
                    "抱著壞掉的手機、日記本與扭曲的生命的種子，你走出了森林。你成為了哥哥純潔罪行下的餘孽，帶著禁忌的種子繼續生存。");
            }
            if (hasPhone && hasDiary && hasWhisper && !GameState.Instance.NancySuicideFlag)
            {
                return new EndingResult("Escape", "成就：埋藏心底",
                    "你帶著壞掉的手機、上鎖的日記與耳語走出了森林，且于晞並未自殺。你選擇將這一切黑暗秘密深埋心底，若無事地回到了人類世界。");
            }
            return null;
        }

        public void UnlockBackgroundStories(Player player, Deck deck)
        {
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyBrokenPhone))
                StoryUnlock.Instance?.UnlockStorySegment("李曉琳背景：壞掉的窺探", "解鎖了李曉琳修復壞掉的手機，暗中窺探哥哥與其他女生曖昧關係的病態日常。");
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyLockedDiary))
                StoryUnlock.Instance?.UnlockStorySegment("李曉琳背景：上鎖的病態", "解鎖了李曉琳日記本中記錄的，對哥哥毫無底線、病態而瘋狂的佔有慾。");
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeySeedOfLife))
                StoryUnlock.Instance?.UnlockStorySegment("李曉琳背景：禁忌的孕育", "解鎖了李曉琳在邪祟蠱惑下，企圖孕育哥哥的生命種子以達成永恆結合的禁忌。");
        }

        public bool HandleNpcEncounter(Player player, Deck deck, CharacterId npcId, int choice)
        {
            if (npcId == CharacterId.Nancy)
            {
                if (choice == 1) // 心機說服 (WIS >= 4 test)
                {
                    int playerWis = TurnManager.Instance.AccumulatedWis;
                    if (playerWis >= 4)
                    {
                        Card phone = CardFactory.CreateCard(CardId.KeyBrokenPhone);
                        if (phone != null) deck.AddCardToDiscardPile(phone);
                        GameState.Instance.AddLog("【智慧說服】你以精妙的心機言辭，誘使 Nancy 交出了「壞掉的手機」！");
                    }
                    else
                    {
                        GameState.Instance.AddLog("【說服失敗】Nancy 充滿戒備，你未能取得她的信賴。");
                    }
                }
                else if (choice == 2) // 用偷拍照片威脅
                {
                    if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeySneakPhoto))
                    {
                        CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeySneakPhoto);
                        Card phone = CardFactory.CreateCard(CardId.KeyBrokenPhone);
                        if (phone != null) deck.AddCardToDiscardPile(phone);
                        
                        GameState.Instance.NancySuicideFlag = true;
                        player.CurrentSanity = Math.Max(0, player.CurrentSanity - 20);
                        
                        GameState.Instance.AddLog("【絕望勒索】你用偷拍照片殘忍勒索 Nancy。交出「壞掉的手機」後，絕望的 Nancy 懸樹自殺！（理智 -20）");
                    }
                    else
                    {
                        GameState.Instance.AddLog("你試圖威脅 Nancy，但你手頭並無「偷拍照片」把柄。");
                    }
                }
                return true;
            }
            else if (npcId == CharacterId.Leo)
            {
                if (player.Corruption > 50 || player.Evil > 50)
                {
                    if (player.CurrentSanity < 20)
                    {
                        Card seed = CardFactory.CreateCard(CardId.KeySeedOfLife);
                        if (seed != null) deck.AddCardToDiscardPile(seed);
                        GameState.Instance.AddLog("【禁忌的種子】在極度狂亂與低理智下，你與哥哥發生了畸形關係，獲得了「生命的種子」！");
                    }
                    else // Sanity >= 20
                    {
                        int playerWis = TurnManager.Instance.AccumulatedWis;
                        if (playerWis >= 5)
                        {
                            Card whisper = CardFactory.CreateCard(CardId.KeyWhisper);
                            if (whisper != null) deck.AddCardToDiscardPile(whisper);
                            GameState.Instance.AddLog("【看破邪祟】你的智慧高超，識破了眼前的邪祟幻影並將其擊退，獲得了「耳語」！");
                        }
                        else
                        {
                            player.CurrentSanity = Math.Max(0, player.CurrentSanity - 20);
                            GameState.Instance.IsDescentActive = true;
                            
                            Card madness = CardFactory.CreateCard(CardId.CurseMadness);
                            if (madness != null) deck.AddCardToDiscardPile(madness);
                            
                            GameState.Instance.AddLog("【邪靈反噬】你未能識破邪祟，精神遭受重創，降神儀式開始！（理智 -20，獲得『狂亂』，神降環境開啟）");
                        }
                    }
                }
                else
                {
                    bool gotSomething = false;
                    if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeySneakPhoto))
                    {
                        CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeySneakPhoto);
                        Card scheme = CardFactory.CreateCard(CardId.KeyScheme);
                        if (scheme != null) deck.AddCardToDiscardPile(scheme);
                        GameState.Instance.AddLog("【取得心機】你交出「偷拍照片」，從哥哥那裡學到了「心機」！");
                        gotSomething = true;
                    }
                    
                    if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyBrokenPhone))
                    {
                        Card diary = CardFactory.CreateCard(CardId.KeyLockedDiary);
                        if (diary != null) deck.AddCardToDiscardPile(diary);
                        GameState.Instance.AddLog("【發現日記】你展示「壞掉的手機」，藉此獲得了「上鎖的日記」！");
                        gotSomething = true;
                    }

                    if (!gotSomething)
                    {
                        GameState.Instance.AddLog("你與哥哥對話，但沒有足夠的證據或物品來獲取任何秘密。");
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
                Card photos = CardFactory.CreateCard(CardId.KeySneakPhoto);
                if (photos != null) deck.AddCardToDiscardPile(photos);
                GameState.Instance.AddLog("【開啟鐵盒】你用老舊鑰匙打開了鐵盒，發現了哥哥隱瞞的【偷拍照片】！");
                return true;
            }
            return false;
        }
    }
}
