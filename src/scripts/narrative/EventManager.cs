using Godot;
using System;
using System.Collections.Generic;
using DeepForest.Character;
using DeepForest.Cards;
using DeepForest.Core;
using DeepForest.Scene;

namespace DeepForest.Narrative
{
    public static class EventManager
    {
        public static EventData? CurrentActiveEvent { get; set; } = null;
        public static List<EventData> TestEvents { get; } = new();

        public static void CheckAndTriggerEvent(MapNode node)
        {
            var player = GameState.Instance.PlayerInstance;
            if (player == null) return;
            var deck = GameState.Instance.DeckInstance;

            // Clean up any old event decals if we are transitioning nodes
            if (CurrentActiveEvent != null)
            {
                string oldDecal = CurrentActiveEvent.DecalName;
                if (!string.IsNullOrEmpty(oldDecal) && node.SceneData != null)
                {
                    node.SceneData.Decals.Remove(oldDecal);
                }
                CurrentActiveEvent = null;
            }

            List<EventData> candidates = new List<EventData>();
            int totalWeight = 0;

            // Add test events first
            foreach (var ev in TestEvents)
            {
                if (DoesEventMatch(ev, node, player))
                {
                    candidates.Add(ev);
                    totalWeight += Math.Max(1, ev.Weight);
                }
            }

            // Scan res://src/resources/events/ if directory exists
            if (DirAccess.DirExistsAbsolute("res://src/resources/events/"))
            {
                var dir = DirAccess.Open("res://src/resources/events/");
                if (dir != null)
                {
                    dir.ListDirBegin();
                    string fileName = dir.GetNext();
                    while (fileName != "")
                    {
                        if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
                        {
                            var ev = GD.Load<EventData>($"res://src/resources/events/{fileName}");
                            if (ev != null && DoesEventMatch(ev, node, player))
                            {
                                candidates.Add(ev);
                                totalWeight += Math.Max(1, ev.Weight);
                            }
                        }
                        fileName = dir.GetNext();
                    }
                    dir.ListDirEnd();
                }
            }

            if (candidates.Count > 0)
            {
                // Weighted selection
                int roll = new Random().Next(0, totalWeight);
                int currentWeightSum = 0;
                EventData selectedEvent = candidates[0];

                foreach (var ev in candidates)
                {
                    currentWeightSum += Math.Max(1, ev.Weight);
                    if (roll < currentWeightSum)
                    {
                        selectedEvent = ev;
                        break;
                    }
                }

                // Set as active event
                CurrentActiveEvent = selectedEvent;

                // Append decal name to scene decals so it renders
                if (!string.IsNullOrEmpty(selectedEvent.DecalName) && node.SceneData != null)
                {
                    if (!node.SceneData.Decals.Contains(selectedEvent.DecalName))
                    {
                        node.SceneData.Decals.Add(selectedEvent.DecalName);
                    }
                }

                // Add log message describing the event start
                GameState.Instance.AddLog($"【事件】{selectedEvent.EventTitle}");
                if (!string.IsNullOrEmpty(selectedEvent.EventDescription))
                {
                    GameState.Instance.AddLog(selectedEvent.EventDescription);
                }

                // Footprint-to-baby-son transformation for Sarah
                if (player.CharacterData?.CharacterId == CharacterId.Sarah && CardQueryHelper.HasCardAnywhere(deck, CardId.KeyTinyFootprints))
                {
                    bool isCorruptionEvent = selectedEvent.RequiredTerrain == "Ruins" 
                        || selectedEvent.RequiredTerrain == "Swamp" 
                        || selectedEvent.RequiredWeather == "Foggy" 
                        || selectedEvent.EventTitle.Contains("穢祟")
                        || selectedEvent.EventTitle.Contains("邪祟")
                        || selectedEvent.EventDescription.Contains("穢祟")
                        || selectedEvent.EventDescription.Contains("邪門");

                    if (isCorruptionEvent)
                    {
                        CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeyTinyFootprints);
                        Card babySon = CardFactory.CreateCard(CardId.KeyBabySon);
                        if (babySon != null)
                        {
                            deck.AddCardToDiscardPile(babySon);
                            player.CurrentSanity = Math.Min(player.MaxSanity, player.CurrentSanity + 20);
                            GameState.Instance.AddLog("【命運交織】在穢祟氣息的侵蝕與幻覺中，你手中的『小小的足跡』化為了【寶貝兒子】！（理智 +20）");
                        }
                    }
                }
            }
        }

        private static bool DoesEventMatch(EventData ev, MapNode node, Player player)
        {
            // 1. Terrain check
            if (!string.IsNullOrEmpty(ev.RequiredTerrain))
            {
                string req = ev.RequiredTerrain.ToLower();
                bool matchesLeft = node.SceneData?.LeftTerrain?.ToLower() == req;
                bool matchesRight = node.SceneData?.RightTerrain?.ToLower() == req;
                if (!matchesLeft && !matchesRight)
                    return false;
            }

            // 2. Weather check
            if (!string.IsNullOrEmpty(ev.RequiredWeather))
            {
                var env = EnvironmentSystem.Instance;
                if (env == null || env.Weather.ToString().ToLower() != ev.RequiredWeather.ToLower())
                    return false;
            }

            // 3. Depth check
            if (node.Depth < ev.MinDepth || node.Depth > ev.MaxDepth)
                return false;

            // 4. Character check
            if (ev.RequiredCharacterId != CharacterId.None)
            {
                if (player.CharacterData == null || player.CharacterData.CharacterId != ev.RequiredCharacterId)
                    return false;
            }

            // 5. HP check
            if (player.CurrentHp < ev.MinHp || player.CurrentHp > ev.MaxHp)
                return false;

            // 6. Sanity check
            if (player.CurrentSanity < ev.MinSanity || player.CurrentSanity > ev.MaxSanity)
                return false;

            // 7. Day check
            if (GameState.Instance != null)
            {
                if (GameState.Instance.CurrentDay < ev.MinDay || GameState.Instance.CurrentDay > ev.MaxDay)
                    return false;
            }

            // 8. Brutality check
            if (player.Brutality < ev.MinBrutality || player.Brutality > ev.MaxBrutality)
                return false;

            // 9. Corruption check
            if (player.Corruption < ev.MinCorruption || player.Corruption > ev.MaxCorruption)
                return false;

            // 10. Evil check
            if (player.Evil < ev.MinEvil || player.Evil > ev.MaxEvil)
                return false;

            // 11. Required card in deck check
            if (ev.RequiredCardId != CardId.None)
            {
                var deck = GameState.Instance.DeckInstance;
                if (deck == null || !CardQueryHelper.HasCardAnywhere(deck, ev.RequiredCardId))
                    return false;
            }

            // 12. Nested composite trigger conditions
            if (ev.CustomCondition != null)
            {
                var deck = GameState.Instance.DeckInstance;
                if (!ev.CustomCondition.Evaluate(player, deck))
                    return false;
            }

            return true;
        }

        public static string ResolveEventOption(EventOption option, Player player, Deck deck)
        {
            // Log Option Chosen
            string eventId = CurrentActiveEvent != null ? CurrentActiveEvent.EventId : "unknown";
            int optionIndex = -1;
            if (CurrentActiveEvent != null)
            {
                optionIndex = CurrentActiveEvent.Options.IndexOf(option);
            }
            GameState.Instance.Logger.LogAction("OptionChosen", new Dictionary<string, object> { 
                { "eventId", eventId }, 
                { "optionIndex", optionIndex },
                { "optionText", option.OptionText }
            });

            // Execute custom nested composite effects
            if (option.Effect != null)
            {
                option.Effect.Execute(player, deck);
            }

            // Apply stat changes
            player.CurrentHp = Math.Clamp(player.CurrentHp + option.HpChange, 0, player.MaxHp);
            player.CurrentSanity = Math.Clamp(player.CurrentSanity + option.SanityChange, 0, player.MaxSanity);
            player.CurrentHunger = Math.Clamp(player.CurrentHunger + option.HungerChange, 0, player.MaxHunger);
            player.CurrentThirst = Math.Clamp(player.CurrentThirst + option.ThirstChange, 0, player.MaxThirst);

            player.Brutality = Math.Clamp(player.Brutality + option.BrutalityChange, 0, 100);
            player.Corruption = Math.Clamp(player.Corruption + option.CorruptionChange, 0, 100);
            player.Evil = Math.Clamp(player.Evil + option.EvilChange, 0, 100);

            // Apply card additions
            if (option.CardIdToGive != CardId.None)
            {
                var card = CardFactory.CreateCard(option.CardIdToGive);
                if (card != null)
                {
                    deck.AddCardToDiscardPile(card);
                }
                else
                {
                    GD.PrintErr($"[EventManager] CardIdToGive failed to create: {option.CardIdToGive}");
                }
            }

            // Apply card removals
            if (option.CardIdToTake != CardId.None)
            {
                var card = CardQueryHelper.FindCardAnywhere(deck, option.CardIdToTake);
                if (card != null)
                {
                    deck.Hand.Remove(card);
                    deck.DrawPile.Remove(card);
                    deck.DiscardPile.Remove(card);
                    deck.EmitSignal(Deck.SignalName.HandChanged);
                    deck.EmitSignal(Deck.SignalName.DeckChanged);
                }
            }

            string log = option.LogMessageOnSuccess;

            // Remove the decal from the active node
            if (CurrentActiveEvent != null)
            {
                string decal = CurrentActiveEvent.DecalName;
                var mapManager = MapManager.Instance;
                if (mapManager != null && mapManager.Nodes.TryGetValue(mapManager.CurrentNodeId, out var node))
                {
                    if (node.SceneData != null && !string.IsNullOrEmpty(decal))
                    {
                        node.SceneData.Decals.Remove(decal);
                    }
                }
                
                // Reset active event
                CurrentActiveEvent = null;
            }

            // Apply next node or indoor scene overrides
            if (option.NextNodeIdOverride >= 0 && MapManager.Instance != null)
            {
                MapManager.Instance.CurrentNodeId = option.NextNodeIdOverride;
            }

            if (!string.IsNullOrEmpty(option.NextIndoorSceneOverride) && MapManager.Instance != null)
            {
                GameState.Instance.IsIndoor = true;
                GameState.Instance.IndoorDepth = 1;
                GameState.Instance.EntranceNodeId = MapManager.Instance.CurrentNodeId;
                
                var indoorScene = MapManager.Instance.GenerateIndoorScene(1);
                if (option.NextIndoorSceneOverride != "auto")
                {
                    indoorScene.SceneName = option.NextIndoorSceneOverride;
                }
                MapManager.Instance.CurrentIndoorScene = indoorScene;
            }

            return log;
        }

        public static void HandleNpcEncounter(Player player, Deck deck, string npcName)
        {
            HandleNpcEncounter(player, deck, npcName, 1);
        }

        public static void HandleNpcEncounter(Player player, Deck deck, string npcName, int choiceIndex)
        {
            CharacterId npcId = npcName switch
            {
                "Sarah" => CharacterId.Sarah,
                "John" => CharacterId.John,
                "Leo" => CharacterId.Leo,
                "Celin" => CharacterId.Celin,
                "Nancy" => CharacterId.Nancy,
                "Tommy" => CharacterId.Tommy,
                _ => CharacterId.None
            };

            if (npcId == CharacterId.None)
            {
                GD.PrintErr($"[EventManager] 無效的 NPC 名字: {npcName}");
                return;
            }

            string charName = player.CharacterData?.CharacterName ?? "預設角色";
            GameState.Instance.AddLog($"【遭遇】{charName} 遇見了 {npcName}。在迷霧的森林中，一個身影站在不遠處。");

            // 連動貼圖渲染：在當前場景加入 NPC 貼圖
            var mapManager = MapManager.Instance;
            if (mapManager != null && mapManager.Nodes.TryGetValue(mapManager.CurrentNodeId, out var node))
            {
                if (node.SceneData != null && !node.SceneData.Decals.Contains("npc_right"))
                {
                    node.SceneData.Decals.Add("npc_right");
                }
            }

            // Delegate to current handler
            if (GameState.Instance.CurrentStoryHandler != null &&
                GameState.Instance.CurrentStoryHandler.HandleNpcEncounter(player, deck, npcId, choiceIndex))
            {
                return;
            }

            // Fallback default NPC dialogues
            switch (npcId)
            {
                case CharacterId.Sarah:
                    player.CurrentSanity = Math.Min(player.MaxSanity, player.CurrentSanity + 10);
                    GameState.Instance.AddLog($"【遭遇 Sarah】她沉穩的聲音讓你感到一絲安慰（理智 +10）。");
                    break;
                case CharacterId.John:
                    player.CurrentHp = Math.Max(0, player.CurrentHp - 5);
                    GameState.Instance.AddLog($"【遭遇 John】他警惕地看著你，氣氛有些緊張（體力 -5）。");
                    break;
                case CharacterId.Leo:
                    player.CurrentSanity = Math.Max(0, player.CurrentSanity - 5);
                    GameState.Instance.AddLog($"【遭遇 Leo】他的神情閃爍不定，顯然有所隱瞞（理智 -5）。");
                    break;
                case CharacterId.Celin:
                    player.Corruption = Math.Min(100, player.Corruption + 5);
                    GameState.Instance.AddLog($"【遭遇 Celin】她神色異樣地自言自語，散發著微弱的穢祟氣息（穢祟 +5）。");
                    break;
                case CharacterId.Nancy:
                    player.CurrentSanity = Math.Max(0, player.CurrentSanity - 10);
                    GameState.Instance.AddLog($"【遭遇 Nancy】她面色蒼白，對你的到來充滿恐懼（理智 -10）。");
                    break;
            }
        }

        public static void HandleWolfVictoryEvent(Player player, Deck deck, int choiceIndex = 1)
        {
            GameState.Instance.AddLog("【戰鬥勝利】你擊敗了攔路的野狼！在狼屍旁，你似乎發現了奇特的痕跡...");
            if (GameState.Instance.CurrentStoryHandler != null &&
                GameState.Instance.CurrentStoryHandler.HandleSpecialEvent(player, deck, "wolf_victory", choiceIndex))
            {
                return;
            }

            // Default fallback
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
                    player.CurrentHp = Math.Max(1, player.CurrentHp - 10);
                    GameState.Instance.AddLog("【強行追蹤】你憑藉靈巧在荊棘中強行搜尋，獲得了「小小的足跡」，但因此被劃傷（體力 -10）。");
                }
                else
                {
                    player.CurrentHp = Math.Max(1, player.CurrentHp - 10);
                    GameState.Instance.AddLog("你強行搜尋，受到荊棘劃傷（體力 -10），但沒能發現任何線索。");
                }
            }
        }

        public static void HandleSelfNpcVictoryEvent(Player player, Deck deck)
        {
            if (GameState.Instance.CurrentStoryHandler != null &&
                GameState.Instance.CurrentStoryHandler.HandleSpecialEvent(player, deck, "self_npc_victory", 1))
            {
                return;
            }

            // Default fallback
            GameState.Instance.AddLog("【幻影破碎】你戰勝了眼前的「自己」！這場內心對決的勝利讓你的意識回歸清明。");
            player.CurrentSanity = Math.Min(player.MaxSanity, player.CurrentSanity + 10);
            Card diagnosis = CardFactory.CreateCard(CardId.KeyDiagnosisCert);
            Card meds = CardFactory.CreateCard(CardId.ConsumableAntidepressant);
            if (diagnosis != null) deck.AddCardToDiscardPile(diagnosis);
            if (meds != null) deck.AddCardToDiscardPile(meds);
            GameState.Instance.AddLog("【尋求診治】醫生 NPC 在幻境深處出現，為你診斷並開了藥物（理智 +10，獲得『診斷證明書』與『抗憂鬱藥物』）。");
        }

        public static void HandleRustyIronBoxEvent(Player player, Deck deck)
        {
            GameState.Instance.AddLog("【密室探索】你在室內場景發現了一個「生鏽鐵盒」，上面掛著一把老舊的銅鎖。");
            
            if (CardQueryHelper.HasCardAnywhere(deck, CardId.KeyOldKey))
            {
                if (GameState.Instance.CurrentStoryHandler != null &&
                    GameState.Instance.CurrentStoryHandler.HandleSpecialEvent(player, deck, "rusty_iron_box", 1))
                {
                    return;
                }
                GameState.Instance.AddLog("你打開了鐵盒，但裡面空無一物。");
            }
            else
            {
                GameState.Instance.AddLog("你試圖打開鐵盒，但你沒有『老舊鑰匙』。");
            }
        }

        public static void HandleDescentCampfireEvent(Player player, Deck deck, int choiceIndex = 1)
        {
            if (GameState.Instance.CurrentStoryHandler != null &&
                GameState.Instance.CurrentStoryHandler.HandleSpecialEvent(player, deck, "descent_campfire", choiceIndex))
            {
                return;
            }

            // Default fallback
            if (GameState.Instance.IsDescentActive)
            {
                if (choiceIndex == 1)
                {
                    CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeyOldScripture);
                    CardQueryHelper.RemoveCardAnywhere(deck, CardId.KeySleazyFlier);
                    GameState.Instance.AddLog("【浴火自新】你將身上的「舊日教本」與「花名冊」丟入點燃的營火中付之一炬！熊熊烈火燒盡了罪孽，帶來了平靜。");
                }
                else
                {
                    GameState.Instance.AddLog("【保留力量】你凝視著火焰，最終決定將教本與名冊緊緊捂在懷中。那股古老的力量似乎與你產生了共鳴...");
                }
            }
            else
            {
                GameState.Instance.AddLog("你在火堆旁取暖，火焰跳躍著，驅散了周圍的寒意。");
            }
        }

        public static void HandleFindDogEvent(Player player, Deck deck, int choiceIndex)
        {
            if (GameState.Instance.CurrentStoryHandler != null &&
                GameState.Instance.CurrentStoryHandler.HandleSpecialEvent(player, deck, "find_dog", choiceIndex))
            {
                return;
            }

            // Default fallback
            var mapManager = MapManager.Instance;
            if (mapManager != null && mapManager.Nodes.TryGetValue(mapManager.CurrentNodeId, out var node))
            {
                if (node.SceneData != null && !node.SceneData.Decals.Contains("npc_right"))
                {
                    node.SceneData.Decals.Add("npc_right");
                }
            }

            if (choiceIndex == 1)
            {
                var jerry = CardFactory.CreateCard(CardId.KeyJerry);
                if (jerry != null)
                {
                    deck.AddCardToDiscardPile(jerry);
                    GameState.Instance.AddLog("【尋狗事件】你找到了真正的「傑利」！牠高興地對你搖尾巴。【傑利】已加入牌組。");
                }
            }
            else if (choiceIndex == 2)
            {
                var jerryFake = CardFactory.CreateCard(CardId.KeyJerryQuestion);
                if (jerryFake != null)
                {
                    deck.AddCardToDiscardPile(jerryFake);
                    GameState.Instance.AddLog("【尋狗事件】你接近了那隻狗，但牠的眼神重疊不定... 牠真的是「傑利？」嗎？【傑利？】已加入牌組。");
                }
            }
        }
    }
}
