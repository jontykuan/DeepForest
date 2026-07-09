using System;
using DeepForest.Cards;
using DeepForest.Character;
using DeepForest.Core;

namespace DeepForest.Narrative.Handlers
{
    public class LeoStoryHandler : ICharacterStoryHandler
    {
        public CharacterId CharacterId => CharacterId.Leo;
        public CharacterStoryFlags Flags { get; } = new();

        public EndingResult? CheckHiddenStatEndings(Player player, Deck deck)
        {
            if (player.Corruption >= 90)
            {
                return new EndingResult("CorruptionEnding", "古神代言人",
                    "你的靈魂已徹底向深淵敞開。你不再是人類，而是成為了舊日支配者在現世的意志延伸，散播著無盡的瘋狂與混亂。");
            }
            if (player.Evil >= 90)
            {
                return new EndingResult("EvilEnding", "幕後黑手",
                    "你以極致的惡意掌控著一切。你利用花名冊與教本，在都市建立起一張龐大的洗腦與犯罪網絡，操控政商名流，成為隱藏在幕後的絕對主宰。");
            }
            return null;
        }

        public EndingResult? CheckEscapeEndings(Player player, Deck deck)
        {
            bool hasScripture = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyOldScripture);
            bool hasRoster = CardQueryHelper.HasCardAnywhere(deck, CardId.KeySleazyFlier) || CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRoster);

            if (GameState.Instance.IsDescentActive)
            {
                if (player.CurrentSanity < 40)
                {
                    return new EndingResult("Disappearance", "李有志結局：降神反噬而死",
                        "在瘋狂的降神儀式中，你直面降臨的邪靈。然而，你脆弱的理智防線被徹底撕裂，肉體在狂亂中乾枯碳化，化為祭壇上的一具無名焦屍。");
                }

                if (!hasScripture && !hasRoster)
                {
                    return new EndingResult("Escape", "李有志結局：直面罪責",
                        "你以強大的意志通過了降神的理智考驗，並在最後關頭將舊日教本與花名冊付之一炬。你選擇向警方自首，解散社團，在漫長的服刑中尋找靈魂的救贖。");
                }
                else
                {
                    return new EndingResult("Escape", "李有志結局：沉淪教主",
                        "你通過了降神的考驗，但選擇保留了力量。你回到都市後，將聯誼社轉型為超自然邪教組織，自己成為了隱藏在都市陰影下的邪靈代言人。");
                }
            }

            if (hasScripture && hasRoster)
            {
                return new EndingResult("Escape", "李有志結局：淫邪魔窟",
                    "你成功帶回了舊日邪教教本與社團花名冊。你將教本中的心靈操縱與邪教洗腦技巧融入聯誼社，建立起更加龐大的地下享樂魔窟，操控著無數迷失的靈魂。");
            }

            return null;
        }

        public void UnlockBackgroundStories(Player player, Deck deck)
        {
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeySleazyFlier) || CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRoster))
                StoryUnlock.Instance?.UnlockStorySegment("李有志背景：聯誼社的支配", "解鎖了李有志利用地下聯誼社與花名冊，支配並威脅多名女大學生的黑暗真相。");
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyOldScripture))
                StoryUnlock.Instance?.UnlockStorySegment("李有志背景：舊日教派傳承", "解鎖了聯誼社前身作為邪教集團，遺留下來的古老舊日教本傳承與邪術。");
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeySneakPhoto))
                StoryUnlock.Instance?.UnlockStorySegment("李有志背景：出軌的把柄", "解鎖了李有志用偷拍照片威脅劉淑莉與于晞，企圖將她們拉入深淵的把柄。");
        }

        public bool HandleNpcEncounter(Player player, Deck deck, CharacterId npcId, int choice)
        {
            if (npcId == CharacterId.Celin)
            {
                if (!CardQueryHelper.HasCardAnywhere(deck, CardId.KeySleazyFlier) && !CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRoster))
                {
                    Card photos = CardFactory.CreateCard(CardId.KeySneakPhoto);
                    if (photos != null) deck.AddCardToDiscardPile(photos);
                    GameState.Instance.AddLog("【妹妹的告發】Celin 遞給了你「偷拍照片」以作警告。（獲得『偷拍照片』）");
                }
                else
                {
                    player.CurrentHp = Math.Max(1, player.CurrentHp - 1);
                    if (choice == 1) // 逃跑
                    {
                        int playerDex = TurnManager.Instance.AccumulatedDex;
                        if (playerDex >= 5)
                        {
                            GameState.Instance.CelinStalkingActive = true;
                            GameState.Instance.CelinStalkDay = GameState.Instance.CurrentDay + 3;
                            GameState.Instance.AddLog("【成功逃離】你躲開了瘋狂的 Celin。但她對你產生了極致的恨意，每 3 天將會現身襲擊你！");
                        }
                        else
                        {
                            player.CurrentSanity = Math.Max(0, player.CurrentSanity - 20);
                            player.Brutality = Math.Min(100, player.Brutality + 20);
                            
                            Card diary = CardFactory.CreateCard(CardId.KeyLockedDiary);
                            if (diary != null) deck.AddCardToDiscardPile(diary);
                            GameState.Instance.AddLog("【逃跑失敗】你逃跑失敗被 Celin 逼入絕境。激戰後 Celin 自殺！（理智 -20，暴戾 +20，獲得『上鎖的日記』）");
                        }
                    }
                    else if (choice == 2) // 擊敗
                    {
                        player.CurrentSanity = Math.Max(0, player.CurrentSanity - 20);
                        player.Brutality = Math.Min(100, player.Brutality + 20);
                        
                        Card diary = CardFactory.CreateCard(CardId.KeyLockedDiary);
                        if (diary != null) deck.AddCardToDiscardPile(diary);
                        GameState.Instance.AddLog("【擊敗妹妹】你無情地擊敗了 Celin。崩潰的她割喉自殺，血濺到你的臉上。（理智 -20，暴戾 +20，獲得『上鎖的日記』）");
                    }
                }
                return true;
            }
            else if (npcId == CharacterId.Nancy)
            {
                if (!CardQueryHelper.HasCardAnywhere(deck, CardId.KeySneakPhoto))
                {
                    player.Brutality = Math.Min(100, player.Brutality + 20);
                    player.Evil = Math.Min(100, player.Evil + 20);
                    GameState.Instance.AddLog("【暴力強逼】你身上沒有照片把柄。你用暴力威脅 Nancy，逼她屈服！（暴戾 +20，邪惡 +20）");
                }
                else
                {
                    CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeySneakPhoto);
                    Card key = CardFactory.CreateCard(CardId.KeyOldKey);
                    if (key != null) deck.AddCardToDiscardPile(key);
                    GameState.Instance.AddLog("【交易把柄】你用身上的「偷拍照片」換取了 Nancy 隱藏的「老舊鑰匙」！");
                }
                return true;
            }
            return false;
        }

        public bool HandleSpecialEvent(Player player, Deck deck, string eventId, int choice)
        {
            if (eventId == "rusty_iron_box")
            {
                Card roster = CardFactory.CreateCard(CardId.KeySleazyFlier);
                if (roster != null) deck.AddCardToDiscardPile(roster);
                GameState.Instance.AddLog("【開啟鐵盒】你用老舊鑰匙打開了鐵盒，發現了塵封的社團【花名冊】！");
                return true;
            }
            else if (eventId == "descent_campfire")
            {
                if (GameState.Instance.IsDescentActive)
                {
                    if (choice == 1) // 燒毀教本與花名冊 -> 贖罪
                    {
                        CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeyOldScripture);
                        if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeySleazyFlier))
                            CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeySleazyFlier);
                        else
                            CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeyRoster);
                        GameState.Instance.AddLog("【浴火自新】你將身上的「舊日教本」與「花名冊」丟入點燃的營火中付之一記！熊熊烈火燒盡了罪孽，帶來了平靜。");
                    }
                    else // 不燒毀 -> 沉淪
                    {
                        GameState.Instance.AddLog("【保留力量】你凝視著火焰，最終決定將教本與名冊緊緊捂在懷中。那股古老的力量似乎與你產生了共鳴...");
                    }
                }
                else
                {
                    GameState.Instance.AddLog("你在火堆旁取暖，火焰跳躍著，驅散了周圍的寒意。");
                }
                return true;
            }
            return false;
        }
    }
}
