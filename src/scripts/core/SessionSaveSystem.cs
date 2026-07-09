using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using DeepForest.Character;
using DeepForest.Cards;
using DeepForest.Scene;

namespace DeepForest.Core
{
    public class SessionSaveData
    {
        public int CurrentDay { get; set; }
        public int CurrentDepth { get; set; }
        public bool IsDescentActive { get; set; }
        public int SarahCampRemovals { get; set; }
        public bool NancySuicideFlag { get; set; }
        public bool CelinStalkingActive { get; set; }
        public int CelinStalkDay { get; set; }
        public bool IsIndoor { get; set; }
        public int IndoorDepth { get; set; }
        public int EntranceNodeId { get; set; }
        public List<string> GameLogs { get; set; } = new();

        public PlayerSaveData Player { get; set; } = new();
        public DeckSaveData Deck { get; set; } = new();
        public MapSaveData Map { get; set; } = new();
    }

    public class PlayerSaveData
    {
        public string CharacterId { get; set; } = "";
        public string CharacterName { get; set; } = "";
        public string CharacterResPath { get; set; } = "";
        public int MaxHp { get; set; }
        public int CurrentHp { get; set; }
        public int MaxSanity { get; set; }
        public int CurrentSanity { get; set; }
        public int MaxHunger { get; set; }
        public int CurrentHunger { get; set; }
        public int MaxThirst { get; set; }
        public int CurrentThirst { get; set; }
        public int Brutality { get; set; }
        public int Corruption { get; set; }
        public int Evil { get; set; }
    }

    public class CardSaveData
    {
        public string CardId { get; set; } = "";
        public int UsesLeft { get; set; }
    }

    public class DeckSaveData
    {
        public List<CardSaveData> DrawPile { get; set; } = new();
        public List<CardSaveData> Hand { get; set; } = new();
        public List<CardSaveData> DiscardPile { get; set; } = new();
        public List<CardSaveData> EquippedCards { get; set; } = new();
    }

    public class SceneActionSaveData
    {
        public string ActionName { get; set; } = "";
        public string ThresholdType { get; set; } = "";
        public int ThresholdValue { get; set; }
        public string RequiredItem { get; set; } = "";
        public string EffectType { get; set; } = "";
        public int HpCostOnComplete { get; set; }
    }

    public class SceneDataSaveData
    {
        public string SceneName { get; set; } = "";
        public string SceneDescription { get; set; } = "";
        public string BottomGround { get; set; } = "";
        public string LeftTerrain { get; set; } = "";
        public string RightTerrain { get; set; } = "";
        public List<string> Decals { get; set; } = new();
        public List<SceneActionSaveData> Actions { get; set; } = new();
    }

    public class MapNodeSaveData
    {
        public int Id { get; set; }
        public int Depth { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; } = "";
        public SceneDataSaveData SceneData { get; set; } = new();
        public List<int> Connections { get; set; } = new();
    }

    public class MapSaveData
    {
        public int CurrentNodeId { get; set; }
        public List<int> ExploredNodeIds { get; set; } = new();
        public SceneDataSaveData? CurrentIndoorScene { get; set; }
        public List<MapNodeSaveData> Nodes { get; set; } = new();
    }

    public static class SessionSaveSystem
    {
        private const string SessionSavePath = "user://deepforest_session.json";

        public static bool HasSessionSave()
        {
            return Godot.FileAccess.FileExists(SessionSavePath);
        }

        public static void DeleteSession()
        {
            try
            {
                if (Godot.FileAccess.FileExists(SessionSavePath))
                {
                    using var dir = Godot.DirAccess.Open("user://");
                    if (dir != null)
                    {
                        dir.Remove("deepforest_session.json");
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[SessionSaveSystem] 刪除存檔失敗: {ex.Message}");
            }
        }

        public static void SaveSession(GameState state, MapManager map)
        {
            if (state == null || map == null) return;
            try
            {
                var data = new SessionSaveData
                {
                    CurrentDay = state.CurrentDay,
                    CurrentDepth = state.CurrentDepth,
                    IsDescentActive = state.IsDescentActive,
                    SarahCampRemovals = state.SarahCampRemovals,
                    NancySuicideFlag = state.NancySuicideFlag,
                    CelinStalkingActive = state.CelinStalkingActive,
                    CelinStalkDay = state.CelinStalkDay,
                    IsIndoor = state.IsIndoor,
                    IndoorDepth = state.IndoorDepth,
                    EntranceNodeId = state.EntranceNodeId,
                    GameLogs = new List<string>(state.GameLogs)
                };

                var p = state.PlayerInstance;
                if (p != null)
                {
                    data.Player = new PlayerSaveData
                    {
                        CharacterId = p.CharacterData?.CharacterId.ToString() ?? "",
                        CharacterName = p.CharacterData?.CharacterName ?? "",
                        CharacterResPath = p.CharacterData?.ResourcePath ?? "",
                        MaxHp = p.MaxHp,
                        CurrentHp = p.CurrentHp,
                        MaxSanity = p.MaxSanity,
                        CurrentSanity = p.CurrentSanity,
                        MaxHunger = p.MaxHunger,
                        CurrentHunger = p.CurrentHunger,
                        MaxThirst = p.MaxThirst,
                        CurrentThirst = p.CurrentThirst,
                        Brutality = p.Brutality,
                        Corruption = p.Corruption,
                        Evil = p.Evil
                    };
                }

                var d = state.DeckInstance;
                if (d != null)
                {
                    data.Deck = new DeckSaveData
                    {
                        DrawPile = MapCardsToPOCO(d.DrawPile),
                        Hand = MapCardsToPOCO(d.Hand),
                        DiscardPile = MapCardsToPOCO(d.DiscardPile),
                        EquippedCards = MapCardsToPOCO(d.EquippedCards)
                    };
                }

                data.Map = new MapSaveData
                {
                    CurrentNodeId = map.CurrentNodeId,
                    ExploredNodeIds = new List<int>(map.ExploredNodeIds),
                    CurrentIndoorScene = map.CurrentIndoorScene != null ? MapSceneDataToPOCO(map.CurrentIndoorScene) : null
                };

                foreach (var node in map.Nodes.Values)
                {
                    data.Map.Nodes.Add(new MapNodeSaveData
                    {
                        Id = node.Id,
                        Depth = node.Depth,
                        X = node.X,
                        Y = node.Y,
                        Name = node.Name,
                        Connections = new List<int>(node.Connections),
                        SceneData = MapSceneDataToPOCO(node.SceneData)
                    });
                }

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                using var file = Godot.FileAccess.Open(SessionSavePath, Godot.FileAccess.ModeFlags.Write);
                if (file != null)
                {
                    file.StoreString(json);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[SessionSaveSystem] 存檔寫入失敗: {ex.Message}");
            }
        }

        public static bool LoadSession(GameState state, MapManager map)
        {
            if (state == null || map == null || !HasSessionSave()) return false;
            try
            {
                string json;
                using (var file = Godot.FileAccess.Open(SessionSavePath, Godot.FileAccess.ModeFlags.Read))
                {
                    if (file == null) return false;
                    json = file.GetAsText();
                }

                var data = JsonSerializer.Deserialize<SessionSaveData>(json);
                if (data == null) return false;

                state.CurrentDay = data.CurrentDay;
                state.CurrentDepth = data.CurrentDepth;
                state.IsDescentActive = data.IsDescentActive;
                state.SarahCampRemovals = data.SarahCampRemovals;
                state.NancySuicideFlag = data.NancySuicideFlag;
                state.CelinStalkingActive = data.CelinStalkingActive;
                state.CelinStalkDay = data.CelinStalkDay;
                state.IsIndoor = data.IsIndoor;
                state.IndoorDepth = data.IndoorDepth;
                state.EntranceNodeId = data.EntranceNodeId;
                state.ClearLogs();
                foreach (var log in data.GameLogs)
                {
                    var listField = typeof(GameState).GetField("_gameLogs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (listField != null)
                    {
                        var list = (List<string>)listField.GetValue(state)!;
                        list.Add(log);
                    }
                }

                var p = state.PlayerInstance;
                if (p != null && data.Player != null)
                {
                    if (!string.IsNullOrEmpty(data.Player.CharacterResPath) && Godot.FileAccess.FileExists(data.Player.CharacterResPath))
                    {
                        var charData = GD.Load<CharacterData>(data.Player.CharacterResPath);
                        p.InitializeFromData(charData);
                    }
                    p.MaxHp = data.Player.MaxHp;
                    p.CurrentHp = data.Player.CurrentHp;
                    p.MaxSanity = data.Player.MaxSanity;
                    p.CurrentSanity = data.Player.CurrentSanity;
                    p.MaxHunger = data.Player.MaxHunger;
                    p.CurrentHunger = data.Player.CurrentHunger;
                    p.MaxThirst = data.Player.MaxThirst;
                    p.CurrentThirst = data.Player.CurrentThirst;
                    p.Brutality = data.Player.Brutality;
                    p.Corruption = data.Player.Corruption;
                    p.Evil = data.Player.Evil;
                }

                var d = state.DeckInstance;
                if (d != null && data.Deck != null)
                {
                    d.DrawPile.Clear();
                    d.DrawPile.AddRange(MapPOCOToCards(data.Deck.DrawPile));

                    d.Hand.Clear();
                    d.Hand.AddRange(MapPOCOToCards(data.Deck.Hand));

                    d.DiscardPile.Clear();
                    d.DiscardPile.AddRange(MapPOCOToCards(data.Deck.DiscardPile));

                    d.EquippedCards.Clear();
                    d.EquippedCards.AddRange(MapPOCOToCards(data.Deck.EquippedCards));

                    d.EmitSignal(Deck.SignalName.HandChanged);
                    d.EmitSignal(Deck.SignalName.DeckChanged);
                }

                if (data.Map != null)
                {
                    map.Nodes.Clear();
                    foreach (var nodePOCO in data.Map.Nodes)
                    {
                        map.Nodes[nodePOCO.Id] = new MapNode
                        {
                            Id = nodePOCO.Id,
                            Depth = nodePOCO.Depth,
                            X = nodePOCO.X,
                            Y = nodePOCO.Y,
                            Name = nodePOCO.Name,
                            Connections = new List<int>(nodePOCO.Connections),
                            SceneData = MapPOCOToSceneData(nodePOCO.SceneData)
                        };
                    }

                    map.ExploredNodeIds.Clear();
                    foreach (var expId in data.Map.ExploredNodeIds)
                    {
                        map.ExploredNodeIds.Add(expId);
                    }

                    map.CurrentIndoorScene = data.Map.CurrentIndoorScene != null ? MapPOCOToSceneData(data.Map.CurrentIndoorScene) : null;
                    map.LoadSetCurrentNodeId(data.Map.CurrentNodeId);
                }

                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[SessionSaveSystem] 讀檔還原失敗: {ex.Message}");
                return false;
            }
        }

        private static List<CardSaveData> MapCardsToPOCO(IEnumerable<Card> cards)
        {
            var list = new List<CardSaveData>();
            foreach (var card in cards)
            {
                if (card == null) continue;
                list.Add(new CardSaveData
                {
                    CardId = card.CardId.ToString(),
                    UsesLeft = card.UsesLeft
                });
            }
            return list;
        }

        private static List<Card> MapPOCOToCards(IEnumerable<CardSaveData> dataList)
        {
            var list = new List<Card>();
            foreach (var cData in dataList)
            {
                if (Enum.TryParse<CardId>(cData.CardId, out var id))
                {
                    var card = CardFactory.CreateCard(id);
                    if (card != null)
                    {
                        card.UsesLeft = cData.UsesLeft;
                        list.Add(card);
                    }
                }
            }
            return list;
        }

        private static SceneDataSaveData MapSceneDataToPOCO(SceneData sd)
        {
            var sdData = new SceneDataSaveData
            {
                SceneName = sd.SceneName,
                SceneDescription = sd.SceneDescription,
                BottomGround = sd.BottomGround,
                LeftTerrain = sd.LeftTerrain,
                RightTerrain = sd.RightTerrain,
                Decals = new List<string>(sd.Decals)
            };

            foreach (var act in sd.Actions)
            {
                if (act == null) continue;
                sdData.Actions.Add(new SceneActionSaveData
                {
                    ActionName = act.ActionName,
                    ThresholdType = act.ThresholdType.ToString(),
                    ThresholdValue = act.ThresholdValue,
                    RequiredItem = act.RequiredItem,
                    EffectType = act.EffectType.ToString(),
                    HpCostOnComplete = act.HpCostOnComplete
                });
            }
            return sdData;
        }

        private static SceneData MapPOCOToSceneData(SceneDataSaveData sdData)
        {
            var sd = new SceneData
            {
                SceneName = sdData.SceneName,
                SceneDescription = sdData.SceneDescription,
                BottomGround = sdData.BottomGround,
                LeftTerrain = sdData.LeftTerrain,
                RightTerrain = sdData.RightTerrain
            };

            foreach (var dec in sdData.Decals)
            {
                sd.Decals.Add(dec);
            }

            foreach (var actData in sdData.Actions)
            {
                if (actData == null) continue;
                sd.Actions.Add(new SceneAction
                {
                    ActionName = actData.ActionName,
                    ThresholdType = Enum.TryParse<ThresholdType>(actData.ThresholdType, out var tt) ? tt : ThresholdType.None,
                    ThresholdValue = actData.ThresholdValue,
                    RequiredItem = actData.RequiredItem,
                    EffectType = Enum.TryParse<ActionEffectType>(actData.EffectType, out var et) ? et : ActionEffectType.None,
                    HpCostOnComplete = actData.HpCostOnComplete
                });
            }
            return sd;
        }
    }
}
