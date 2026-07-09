using System;
using DeepForest.Cards;
using DeepForest.Character;
using DeepForest.Core;

namespace DeepForest.Narrative.Handlers
{
    public class NancyStoryHandler : ICharacterStoryHandler
    {
        public CharacterId CharacterId => CharacterId.Nancy;
        public CharacterStoryFlags Flags { get; } = new();

        public EndingResult? CheckHiddenStatEndings(Player player, Deck deck)
        {
            if (player.Brutality >= 90)
            {
                return new EndingResult("BrutalityEnding", "狂亂的復仇者",
                    "無休止的痛苦與背叛讓你徹底瘋狂。你將所有遇到的人視為仇敵，以殘忍的手段折磨任何落入你手中的生命。");
            }
            if (player.Corruption >= 90)
            {
                return new EndingResult("CorruptionEnding", "深淵傀儡",
                    "你心中的防線被徹底攻破，主動向森林中的神明獻祭了靈魂。你成為了沒有自我的空殼，永遠徘徊在深淵的邊緣。");
            }
            if (player.Evil >= 90)
            {
                return new EndingResult("EvilEnding", "黑暗教唆者",
                    "你雖然活了下來，但心靈已徹底扭曲。你取代了原來的教主，開始用謊言與恐懼操控他人，成為新一代的邪教教唆者。");
            }
            return null;
        }

        public EndingResult? CheckEscapeEndings(Player player, Deck deck)
        {
            bool hasPill = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyEmptyPillBottle);
            bool hasDiag = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyDiagnosisCert);
            bool hasPhoto = CardQueryHelper.HasCardAnywhere(deck, CardId.KeySneakPhoto);

            bool hasRoster = CardQueryHelper.HasCardAnywhere(deck, CardId.KeySleazyFlier) || CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRoster);
            bool hasShowdown = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyShowdown);
            bool hasTruth = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyPainfulTruth);

            if (hasPill && hasDiag && hasPhoto)
            {
                return new EndingResult("Escape", "成就：雨天娃娃",
                    "在痛苦與憂鬱的重壓下，你只帶著空藥瓶、診斷書和偷拍照片走出了森林。你就像一個高掛在屋簷下的雨天娃娃，靈魂早已死在雨中。");
            }
            if (hasRoster && hasShowdown && hasTruth)
            {
                return new EndingResult("Escape", "成就：自我拯救",
                    "手握著花名冊、攤牌與痛苦真相，你成功地揭露了所有黑暗，並完成了對自我的救贖。你重獲新生，走出了迷霧。");
            }
            return null;
        }

        public void UnlockBackgroundStories(Player player, Deck deck)
        {
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeySleazyFlier) || CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRoster))
                StoryUnlock.Instance?.UnlockStorySegment("于晞背景：名冊的控制", "解鎖了于晞被李有志用社團花名冊勒索、控制的屈辱過去。");
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyShowdown))
                StoryUnlock.Instance?.UnlockStorySegment("于晞背景：最後的反抗", "解鎖了于晞收集所有證據，準備與李有志徹底攤牌、重獲自由的決意。");
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyPainfulTruth))
                StoryUnlock.Instance?.UnlockStorySegment("于晞背景：夢魘的真相", "解鎖了于晞發現自己一直以來的痛苦遭遇，皆是人為操縱與背叛的痛苦真相。");
        }

        public bool HandleNpcEncounter(Player player, Deck deck, CharacterId npcId, int choice)
        {
            if (npcId == CharacterId.Leo)
            {
                int injuryCount = CardQueryHelper.CountInjuryCards(deck);
                if (injuryCount > 10)
                {
                    Card roster = CardFactory.CreateCard(CardId.KeySleazyFlier);
                    if (roster != null) deck.AddCardToDiscardPile(roster);
                    GameState.Instance.AddLog("【傷痕累累】看見你滿身的傷痕（超過 10 張傷勢卡），Leo 對你徹底失去興趣，隨手將「花名冊」丟給了你！");
                }
                else
                {
                    int playerDex = TurnManager.Instance.AccumulatedDex;
                    if (playerDex >= 5)
                    {
                        Card roster = CardFactory.CreateCard(CardId.KeySleazyFlier);
                        if (roster != null) deck.AddCardToDiscardPile(roster);
                        GameState.Instance.AddLog("【竊取成功】你憑藉敏捷的靈巧，成功偷走了 Leo 懷中的「花名冊」！");
                    }
                    else
                    {
                        GameState.Instance.AddLog("【竊取失敗】你的靈巧不足，沒能拿到花名冊。");
                    }
                }
                return true;
            }
            else if (npcId == CharacterId.Celin)
            {
                if (!CardQueryHelper.HasCardAnywhere(deck, CardId.KeySleazyFlier) && !CardQueryHelper.HasCardAnywhere(deck, CardId.KeyRoster))
                {
                    Card photos = CardFactory.CreateCard(CardId.KeySneakPhoto);
                    if (photos != null) deck.AddCardToDiscardPile(photos);
                    GameState.Instance.AddLog("【勒索把柄】由於你未持有花名冊，Celin 給予你「偷拍照片」進行勒索威脅！（獲得無法移除的『偷拍照片』）");
                }
                else
                {
                    int playerDex = TurnManager.Instance.AccumulatedDex;
                    if (playerDex >= 5)
                    {
                        player.CurrentSanity = Math.Max(0, player.CurrentSanity - 20);
                        player.Brutality = Math.Min(100, player.Brutality + 20);
                        
                        Card diary = CardFactory.CreateCard(CardId.KeyLockedDiary);
                        if (diary != null) deck.AddCardToDiscardPile(diary);
                        
                        GameState.Instance.NancySuicideFlag = true;
                        GameState.Instance.AddLog("【極端結局】你持有花名冊與 Celin 對決。你的靈巧高超將她擊敗/逃脫，驚恐絕望的 Celin 在狂亂中選擇了自殺。（理智 -20，暴戾 +20，獲得『上鎖的日記』）");
                    }
                    else
                    {
                        player.CurrentHp = Math.Max(1, player.CurrentHp - 15);
                        GameState.Instance.AddLog("【落敗受傷】你的靈巧不足以避開瘋狂的 Celin，受到重創（體力 -15）。");
                    }
                }
                return true;
            }
            return false;
        }

        public bool HandleSpecialEvent(Player player, Deck deck, string eventId, int choice)
        {
            if (eventId == "self_npc_victory")
            {
                GameState.Instance.AddLog("【幻影破碎】你戰勝了眼前的「自己」！這場內心對決的勝利讓你的意識回歸清明。");
                
                player.CurrentSanity = Math.Min(player.MaxSanity, player.CurrentSanity + 10);
                
                Card diagnosis = CardFactory.CreateCard(CardId.KeyDiagnosisCert);
                Card meds = CardFactory.CreateCard(CardId.ConsumableAntidepressant);
                
                if (diagnosis != null) deck.AddCardToDiscardPile(diagnosis);
                if (meds != null) deck.AddCardToDiscardPile(meds);
                
                GameState.Instance.AddLog("【尋求診治】醫生 NPC 在幻境深處出現，為你診斷並開了藥物（理智 +10，獲得『診斷證明書』與『抗憂鬱藥物』）。");
                return true;
            }
            return false;
        }
    }
}
