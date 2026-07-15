using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DeepForest.Character;
using DeepForest.Cards;

namespace DeepForest.Core
{
    public class ActionLogEntry
    {
        [JsonPropertyName("sid")]
        public string SessionId { get; set; } = "";

        [JsonPropertyName("seq")]
        public int SeqNo { get; set; }

        [JsonPropertyName("ts")]
        public long Timestamp { get; set; }

        [JsonPropertyName("day")]
        public int Day { get; set; }

        [JsonPropertyName("depth")]
        public int Depth { get; set; }

        [JsonPropertyName("nodeId")]
        public int NodeId { get; set; }

        [JsonPropertyName("action")]
        public string ActionType { get; set; } = "";

        [JsonPropertyName("params")]
        public Dictionary<string, object> Params { get; set; } = new();

        [JsonPropertyName("delta")]
        public Dictionary<string, int> Delta { get; set; } = new();

        [JsonPropertyName("hand")]
        public List<string> Hand { get; set; } = new();

        [JsonPropertyName("snapshot")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DaySnapshotSnapshot? Snapshot { get; set; }
    }

    public class DaySnapshotSnapshot
    {
        [JsonPropertyName("hp")]
        public int Hp { get; set; }
        
        [JsonPropertyName("sanity")]
        public int Sanity { get; set; }

        [JsonPropertyName("hunger")]
        public int Hunger { get; set; }

        [JsonPropertyName("thirst")]
        public int Thirst { get; set; }

        [JsonPropertyName("brutality")]
        public int Brutality { get; set; }

        [JsonPropertyName("corruption")]
        public int Corruption { get; set; }

        [JsonPropertyName("evil")]
        public int Evil { get; set; }

        [JsonPropertyName("drawPile")]
        public List<string> DrawPile { get; set; } = new();

        [JsonPropertyName("discardPile")]
        public List<string> DiscardPile { get; set; } = new();

        [JsonPropertyName("equippedIds")]
        public List<string> EquippedIds { get; set; } = new();

        [JsonPropertyName("weather")]
        public string Weather { get; set; } = "";

        [JsonPropertyName("temperature")]
        public string Temperature { get; set; } = "";

        [JsonPropertyName("humidity")]
        public string Humidity { get; set; } = "";
    }

    public class ActionLogger
    {
        private static readonly char[] Base36Alphabet = "0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray();

        public string SessionId { get; private set; } = "";
        public int NextSeqNo { get; private set; } = 0;
        public string LogFilePath { get; private set; } = "";
        
        private readonly List<ActionLogEntry> _memoryCache = new();
        private Dictionary<string, int> _lastStats = new();

        public ActionLogger()
        {
            InitializeNewSession();
        }

        public void InitializeNewSession()
        {
            SessionId = GenerateBase36Id(5);
            NextSeqNo = 0;
            _memoryCache.Clear();

            string logsDir = "user://logs/sessions";
            using var dir = DirAccess.Open("user://");
            if (dir != null)
            {
                if (!dir.DirExists("logs"))
                {
                    dir.MakeDir("logs");
                }
                using var dirLogs = DirAccess.Open("user://logs");
                if (dirLogs != null && !dirLogs.DirExists("sessions"))
                {
                    dirLogs.MakeDir("sessions");
                }
            }

            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            LogFilePath = $"{logsDir}/session_{SessionId}_{unixTimestamp}.jsonl";
            
            CaptureLastStats();
        }

        private string GenerateBase36Id(int length)
        {
            var random = new Random();
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = Base36Alphabet[random.Next(Base36Alphabet.Length)];
            }
            return new string(result);
        }

        private void CaptureLastStats()
        {
            _lastStats.Clear();
            var player = GameState.Instance?.PlayerInstance;
            if (player != null)
            {
                _lastStats["hp"] = player.CurrentHp;
                _lastStats["sanity"] = player.CurrentSanity;
                _lastStats["hunger"] = player.CurrentHunger;
                _lastStats["thirst"] = player.CurrentThirst;
                _lastStats["brutality"] = player.Brutality;
                _lastStats["corruption"] = player.Corruption;
                _lastStats["evil"] = player.Evil;
            }
            else
            {
                _lastStats["hp"] = 100;
                _lastStats["sanity"] = 100;
                _lastStats["hunger"] = 100;
                _lastStats["thirst"] = 100;
                _lastStats["brutality"] = 0;
                _lastStats["corruption"] = 0;
                _lastStats["evil"] = 0;
            }
        }

        public void LogAction(string actionType, Dictionary<string, object> parameters)
        {
            var player = GameState.Instance?.PlayerInstance;
            var deck = GameState.Instance?.DeckInstance;
            int day = GameState.Instance?.CurrentDay ?? 1;
            int depth = GameState.Instance?.CurrentDepth ?? 0;
            int nodeId = Scene.MapManager.Instance?.CurrentNodeId ?? 0;

            var entry = new ActionLogEntry
            {
                SessionId = SessionId,
                SeqNo = NextSeqNo++,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Day = day,
                Depth = depth,
                NodeId = nodeId,
                ActionType = actionType,
                Params = parameters ?? new Dictionary<string, object>()
            };

            // Capture Delta
            if (player != null)
            {
                var stats = new Dictionary<string, int>
                {
                    { "hp", player.CurrentHp },
                    { "sanity", player.CurrentSanity },
                    { "hunger", player.CurrentHunger },
                    { "thirst", player.CurrentThirst },
                    { "brutality", player.Brutality },
                    { "corruption", player.Corruption },
                    { "evil", player.Evil }
                };

                foreach (var kvp in stats)
                {
                    int lastVal = _lastStats.ContainsKey(kvp.Key) ? _lastStats[kvp.Key] : kvp.Value;
                    int diff = kvp.Value - lastVal;
                    if (diff != 0)
                    {
                        entry.Delta[kvp.Key] = diff;
                    }
                    _lastStats[kvp.Key] = kvp.Value;
                }
            }

            // Capture Hand
            if (deck != null)
            {
                foreach (var card in deck.Hand)
                {
                    if (card != null)
                    {
                        entry.Hand.Add(card.CardId.ToString());
                    }
                }
            }

            // DayChanged snapshot
            if (actionType == "DayChanged" && player != null && deck != null)
            {
                var snapshot = new DaySnapshotSnapshot
                {
                    Hp = player.CurrentHp,
                    Sanity = player.CurrentSanity,
                    Hunger = player.CurrentHunger,
                    Thirst = player.CurrentThirst,
                    Brutality = player.Brutality,
                    Corruption = player.Corruption,
                    Evil = player.Evil,
                    Weather = EnvironmentSystem.Instance?.Weather.ToString() ?? "",
                    Temperature = EnvironmentSystem.Instance?.Temperature.ToString() ?? "",
                    Humidity = EnvironmentSystem.Instance?.Humidity.ToString() ?? ""
                };

                foreach (var card in deck.DrawPile)
                {
                    if (card != null) snapshot.DrawPile.Add(card.CardId.ToString());
                }
                foreach (var card in deck.DiscardPile)
                {
                    if (card != null) snapshot.DiscardPile.Add(card.CardId.ToString());
                }
                foreach (var card in deck.EquippedCards)
                {
                    if (card != null) snapshot.EquippedIds.Add(card.CardId.ToString());
                }

                entry.Snapshot = snapshot;
            }

            _memoryCache.Add(entry);

            // Serialize to file
            AppendToFile(entry);
        }

        private void AppendToFile(ActionLogEntry entry)
        {
            try
            {
                string jsonLine = JsonSerializer.Serialize(entry);
                using var file = Godot.FileAccess.Open(LogFilePath, Godot.FileAccess.ModeFlags.ReadWrite);
                if (file == null)
                {
                    using var newFile = Godot.FileAccess.Open(LogFilePath, Godot.FileAccess.ModeFlags.Write);
                    if (newFile != null)
                    {
                        newFile.StoreLine(jsonLine);
                    }
                }
                else
                {
                    file.SeekEnd();
                    file.StoreLine(jsonLine);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to append action log to file: {ex.Message}");
            }
        }

        // Methods for in-memory query evaluation
        public bool HasLog(string actionType, string paramKey, object paramValue, string scope)
        {
            int currentDay = GameState.Instance?.CurrentDay ?? 1;
            int currentNodeId = Scene.MapManager.Instance?.CurrentNodeId ?? 0;

            for (int i = _memoryCache.Count - 1; i >= 0; i--)
            {
                var log = _memoryCache[i];
                
                // Scope filtration
                if (scope == "ThisScene" && log.NodeId != currentNodeId) continue;
                if (scope == "ThisTurn" && log.Day != currentDay) continue; // Note: Day matches Turn boundaries

                if (log.ActionType == actionType)
                {
                    if (log.Params.TryGetValue(paramKey, out var val) && val != null && val.ToString() == paramValue.ToString())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int LogCount(string actionType, string scope)
        {
            int currentDay = GameState.Instance?.CurrentDay ?? 1;
            int currentNodeId = Scene.MapManager.Instance?.CurrentNodeId ?? 0;
            int count = 0;

            foreach (var log in _memoryCache)
            {
                if (scope == "ThisScene" && log.NodeId != currentNodeId) continue;
                if (scope == "ThisTurn" && log.Day != currentDay) continue;

                if (log.ActionType == actionType)
                {
                    count++;
                }
            }
            return count;
        }

        public List<ActionLogEntry> GetMemoryLogs()
        {
            return _memoryCache;
        }
    }
}
