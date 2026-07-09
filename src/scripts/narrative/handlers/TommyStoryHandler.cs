using System;
using DeepForest.Cards;
using DeepForest.Character;
using DeepForest.Core;
using DeepForest.Scene;
using DeepForest.Combat;
using Godot;

namespace DeepForest.Narrative.Handlers
{
    public class TommyStoryHandler : ICharacterStoryHandler
    {
        public CharacterId CharacterId => CharacterId.Tommy;
        public CharacterStoryFlags Flags { get; } = new();

        public EndingResult? CheckHiddenStatEndings(Player player, Deck deck)
        {
            if (player.Brutality >= 90)
            {
                return new EndingResult("BrutalityEnding", "荒野小野獸",
                    "極度的恐懼與孤獨將你撕裂。你拋棄了所有人類的情感與語言，四肢著地，如同一隻真正的野獸野狗一般，在林間尋求著最原始的生存。");
            }
            if (player.Corruption >= 90)
            {
                bool hasCollar = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyJerryCollar);
                bool hasFootprints = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyTinyFootprints);
                bool hasFakeJerry = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyJerryQuestion);
                if (hasCollar && hasFootprints && hasFakeJerry)
                {
                    return new EndingResult("CorruptionEnding", "成就：新朋友",
                        "你抱著那隻散發詭異不祥氣息的「小狗」，和地上的小足跡。穢祟黑絲將你們徹底纏繞，你微笑著迎來了你的新朋友，融為一體...");
                }

                return new EndingResult("CorruptionEnding", "畸形的擁抱",
                    "你抱著那隻不再溫暖的『小狗』，任由林間的黑絲與觸手將你們纏繞。你微笑著與牠融為一體，沉入不祥而溫暖的幽暗深淵...");
            }
            if (player.Evil >= 90)
            {
                return new EndingResult("EvilEnding", "冷酷的玩偶師",
                    "走出森林的你，雙眼徹底失去了純真。你學會了用無邪的笑容隱藏極端的冷酷，把身邊的所有人當作可以隨意擺弄、甚至棄置的玩偶。");
            }
            return null;
        }

        public EndingResult? CheckEscapeEndings(Player player, Deck deck)
        {
            bool hasJerry = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyJerry);
            bool hasFakeJerry = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyJerryQuestion);
            bool hasCollar = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyJerryCollar);
            bool hasFootprints = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyTinyFootprints);
            bool hasCopy = CardQueryHelper.HasCardAnywhere(deck, CardId.ActionCopy);
            bool hasTornBook = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyTornFairytale);
            bool hasSuffocation = CardQueryHelper.HasCardAnywhere(deck, CardId.CurseSuffocation);
            int depth = GameState.Instance.CurrentDepth;

            if (hasSuffocation && hasCollar)
            {
                return new EndingResult("Escape", "成就：好狗狗",
                    "在被「窒息」折磨的同時，你選擇戴上了傑利的項圈... 成為一隻乖狗。你回到了人類世界，但靈魂已被徹底馴服。");
            }

            if (hasCollar && hasFootprints && hasJerry && depth > 100)
            {
                return new EndingResult("Escape", "成就：湯姆與傑利",
                    "你帶著傑利的項圈，踩著小小的足跡，在真正的忠實夥伴「傑利」陪伴下，走出了深度大於 100 的森林深處。你們迎著陽光，開啟了全新的冒險！");
            }

            if (hasCopy && hasTornBook)
            {
                return new EndingResult("Escape", "成就：暴力循環",
                    "破損的童話書上記錄著被拷貝的暴力，而你選擇了「複製」這一切。你回歸人類世界後，終將走上父親的老路，重複著殘忍的暴力循環。");
            }

            if (hasJerry)
            {
                return new EndingResult("Escape", "湯明亮結局：傑利與我",
                    "在忠誠小狗傑利的陪伴下，你勇敢地穿過了濃霧，回到了家人的懷抱。傑利蹭著你的小手，發出溫暖的叫聲。");
            }
            else if (hasFakeJerry)
            {
                return new EndingResult("Escape", "湯明亮結局：扭曲的幻影",
                    "你帶著那隻『小狗』回到了人類世界。表面上逃出了森林，但每次看著小狗空洞的雙眼，你總會感到一陣深入骨髓的陰冷...");
            }
            else
            {
                return new EndingResult("Escape", "湯明亮結局：孤單的倖存者",
                    "你獨自一人回到了陽光下。雖然活了下來，但你永遠失去了心愛的小狗，童年的陰影將伴隨你一生。");
            }
        }

        public void UnlockBackgroundStories(Player player, Deck deck)
        {
        }

        public bool HandleNpcEncounter(Player player, Deck deck, CharacterId npcId, int choice)
        {
            if (npcId == CharacterId.Celin)
            {
                GameState.Instance.AddLog("【遭遇 Celin】李曉琳面帶微笑地揮舞著餐刀向你刺來！");
                GameState.Instance.AddLog("【第一回合對決】你勉強擋住了她的刀鋒，李曉琳卻突然收回了手。");
                GameState.Instance.AddLog("「好孩子，這個給你。」她遞給你一塊巧克力後，轉身消失在濃霧中。");
                
                Card chocolate = CardFactory.CreateCard(CardId.ConsumableChocolate);
                if (chocolate != null) deck.AddCardToDiscardPile(chocolate);
                return true;
            }

            if (npcId == CharacterId.John)
            {
                if (choice == 1) // 逃跑
                {
                    int playerDex = TurnManager.Instance.AccumulatedDex;
                    if (playerDex >= 5)
                    {
                        Card rebellion = CardFactory.CreateCard(CardId.ActionRebellion);
                        if (rebellion != null) deck.AddCardToDiscardPile(rebellion);
                        GameState.Instance.AddLog("【逃離父親】你憑著敏捷的身手，在樹叢中擺脫了父親。心中湧現出強烈的【反抗】意志！");
                    }
                    else
                    {
                        Card copy = CardFactory.CreateCard(CardId.ActionCopy);
                        if (copy != null) deck.AddCardToDiscardPile(copy);
                        player.Brutality = Math.Min(100, player.Brutality + 3);
                        GameState.Instance.AddLog("【逃離失敗】你的速度不夠快，被父親粗暴地拽住。在被壓抑的痛楚中，你學會了【複製】（暴戾 +3）。");
                    }
                }
                else
                {
                    Card copy = CardFactory.CreateCard(CardId.ActionCopy);
                    if (copy != null) deck.AddCardToDiscardPile(copy);
                    player.Brutality = Math.Min(100, player.Brutality + 3);
                    GameState.Instance.AddLog("【未逃跑】你未進行逃跑便離開了，父親的陰影重重壓下，你學會了【複製】以求自保（暴戾 +3）。");
                }
                return true;
            }

            if (npcId == CharacterId.Sarah)
            {
                bool hasCollar = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyJerryCollar);
                if (!hasCollar)
                {
                    Card suffocation = CardFactory.CreateCard(CardId.CurseSuffocation);
                    if (suffocation != null) deck.AddCardToHand(suffocation);
                    GameState.Instance.AddLog("【遭遇 Sarah】你看著憤怒而空洞的母親，感到呼吸急促窒息。獲得了詛咒卡【窒息】！");
                }
                else
                {
                    GameState.Instance.AddLog("【遭遇 Sarah】母親伸出雙手試圖奪走你脖子上的傑利項圈，你被迫進行反抗！");
                    var sarahEnemy = EnemyFactory.GetEnemyDataForDecal("combat_sarah");
                    CombatManager.Instance.StartCombat(sarahEnemy);
                }
                return true;
            }

            return false;
        }

        public bool HandleSpecialEvent(Player player, Deck deck, string eventId, int choice)
        {
            if (eventId == "find_dog")
            {
                if (choice != 1 && choice != 2)
                {
                    GD.Print($"[TommyStoryHandler] 尋狗事件：無效的選擇 {choice}");
                    return true;
                }

                var mapManager = MapManager.Instance;
                if (mapManager != null && mapManager.Nodes.TryGetValue(mapManager.CurrentNodeId, out var node))
                {
                    if (node.SceneData != null && !node.SceneData.Decals.Contains("npc_right"))
                    {
                        node.SceneData.Decals.Add("npc_right");
                    }
                }

                if (choice == 1)
                {
                    var jerry = CardFactory.CreateCard(CardId.KeyJerry);
                    if (jerry != null)
                    {
                        deck.AddCardToDiscardPile(jerry);
                        GameState.Instance.AddLog("【尋狗事件】你找到了真正的「傑利」！牠高興地對你搖尾巴。【傑利】已加入牌組。");
                    }
                    else
                    {
                        GD.PrintErr("[TommyStoryHandler] 找不到 card_jerry 資源。");
                    }
                }
                else if (choice == 2)
                {
                    var jerryFake = CardFactory.CreateCard(CardId.KeyJerryQuestion);
                    if (jerryFake != null)
                    {
                        deck.AddCardToDiscardPile(jerryFake);
                        GameState.Instance.AddLog("【尋狗事件】你接近了那隻狗，但牠的眼神重疊不定... 牠真的是「傑利？」嗎？【傑利？】已加入牌組。");
                    }
                    else
                    {
                        GD.PrintErr("[TommyStoryHandler] 找不到 card_jerry_fake 資源。");
                    }
                }
                return true;
            }

            if (eventId == "rusty_iron_box")
            {
                if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyOldKey))
                {
                    Card tornBook = CardFactory.CreateCard(CardId.KeyTornFairytale);
                    if (tornBook != null)
                    {
                        deck.AddCardToDiscardPile(tornBook);
                        GameState.Instance.AddLog("【密室探索】你用老舊鑰匙打開了生鏽鐵盒，在其中找到了一本【破損圖畫書】（撕毀的童話書）！");
                    }
                    return true;
                }
            }

            if (eventId == "wolf_victory")
            {
                bool hasCollar = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyJerryCollar);
                bool hasFootprints = CardQueryHelper.HasCardAnywhere(deck, CardId.KeyTinyFootprints);

                if (!hasCollar)
                {
                    Card collar = CardFactory.CreateCard(CardId.KeyJerryCollar);
                    if (collar != null) deck.AddCardToDiscardPile(collar);
                    GameState.Instance.AddLog("【戰鬥勝利】在狼屍旁，你發現了一個沾著血跡的紅項圈，上面刻著傑利的名字。【傑利的項圈】放入了背包！");
                    return true;
                }
                else if (hasFootprints)
                {
                    GameState.Instance.AddLog("【是你嗎？】在被擊敗的狼屍後方，迷霧中浮現出一個高大、長滿觸手的陰影...");
                    GameState.Instance.AddLog("陰影突然發狂，對你發動了 3 次猛烈的精神衝擊與觸手抽打！");
                    
                    player.CurrentHp -= 30;
                    GameState.Instance.AddLog("【遭受襲擊】你失去了 30 點體力！");
                    
                    if (player.CurrentHp <= 0)
                    {
                        GameState.Instance.AddLog("你無法抵擋這股狂亂的力量，在黑暗中倒下了...");
                        return true;
                    }

                    if (player.CurrentSanity >= 30)
                    {
                        Card jerry = CardFactory.CreateCard(CardId.KeyJerry);
                        if (jerry != null) deck.AddCardToDiscardPile(jerry);
                        GameState.Instance.AddLog("【奇蹟重逢】你恢復了神智。高大陰影消退，顯現出小狗傑利清澈的雙眼！真的是牠！【傑利】加入了背包！");
                    }
                    else
                    {
                        Card fakeJerry = CardFactory.CreateCard(CardId.KeyJerryQuestion);
                        if (fakeJerry != null) deck.AddCardToDiscardPile(fakeJerry);
                        GameState.Instance.AddLog("【詭異重逢】你的意志陷入模糊。陰影化成了一隻長著數隻眼睛、低聲囈語的「傑利？」。【傑利？】加入了背包！");
                    }
                    return true;
                }
            }

            return false;
        }
    }
}
