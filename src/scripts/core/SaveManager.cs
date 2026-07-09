using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DeepForest.Core
{
    public class SaveData
    {
        public string SelectedCharacter { get; set; } = "劉淑莉";
        public Dictionary<string, List<string>> UnlockedEndings { get; set; } = new();
        public Dictionary<string, string> CollectedItems { get; set; } = new();
        public Dictionary<string, List<string>> UnlockedMechanisms { get; set; } = new();
        public float VolumeMaster { get; set; } = 0.8f;
        public float VolumeBgm { get; set; } = 0.8f;
        public float VolumeSfx { get; set; } = 0.8f;
        public string WindowSize { get; set; } = "1280x720";
    }

    public static class SaveManager
    {
        private const string SavePath = "user://deepforest_save.json";

        public static SaveData CurrentSave { get; private set; } = new();

        static SaveManager()
        {
            Load();
        }

        public static void ApplySettings()
        {
            ApplyBusVolume("Master", CurrentSave.VolumeMaster);
            ApplyBusVolume("Music", CurrentSave.VolumeBgm);
            ApplyBusVolume("SFX", CurrentSave.VolumeSfx);
            ApplyWindowSize(CurrentSave.WindowSize);
        }

        public static void ApplyBusVolume(string busName, float ratio)
        {
            int index = AudioServer.GetBusIndex(busName);
            if (index == -1)
            {
                AudioServer.AddBus();
                index = AudioServer.GetBusCount() - 1;
                AudioServer.SetBusName(index, busName);
            }
            float db = Mathf.LinearToDb(ratio);
            AudioServer.SetBusVolumeDb(index, db);
            AudioServer.SetBusMute(index, ratio <= 0.001f);
        }

        public static void ApplyWindowSize(string sizeStr)
        {
            if (sizeStr == "Fullscreen")
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
            else
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                var parts = sizeStr.Split('x');
                if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                {
                    DisplayServer.WindowSetSize(new Vector2I(w, h));
                    
                    var screenId = DisplayServer.WindowGetCurrentScreen();
                    var screenSize = DisplayServer.ScreenGetSize(screenId);
                    var windowSize = DisplayServer.WindowGetSize();
                    DisplayServer.WindowSetPosition(new Vector2I(
                        (screenSize.X - windowSize.X) / 2,
                        (screenSize.Y - windowSize.Y) / 2
                    ));
                }
            }
        }

        public static void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(CurrentSave, new JsonSerializerOptions { WriteIndented = true });
                using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write);
                if (file != null)
                {
                    file.StoreString(json);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[SaveManager] 存檔失敗: {ex.Message}");
            }
        }

        public static void Load()
        {
            try
            {
                if (!Godot.FileAccess.FileExists(SavePath))
                {
                    CurrentSave = new SaveData();
                    return;
                }

                using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Read);
                if (file != null)
                {
                    string json = file.GetAsText();
                    CurrentSave = JsonSerializer.Deserialize<SaveData>(json) ?? new SaveData();
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[SaveManager] 讀檔失敗: {ex.Message}");
                CurrentSave = new SaveData();
            }
        }

        public static void UnlockEnding(string characterName, string endingTitle)
        {
            if (!CurrentSave.UnlockedEndings.ContainsKey(characterName))
            {
                CurrentSave.UnlockedEndings[characterName] = new List<string>();
            }

            if (!CurrentSave.UnlockedEndings[characterName].Contains(endingTitle))
            {
                CurrentSave.UnlockedEndings[characterName].Add(endingTitle);
                Save();
                GameState.Instance.AddLog($"【故事解鎖】{characterName} 已解鎖結局：{endingTitle}");
            }
        }

        public static void CollectItem(string itemName, string sourcePath)
        {
            if (!CurrentSave.CollectedItems.ContainsKey(itemName))
            {
                CurrentSave.CollectedItems[itemName] = sourcePath;
                Save();
                GameState.Instance.AddLog($"【劇情物品】收集到重要物品：{itemName}（解鎖途徑：{sourcePath}）");
            }
        }

        public static void UnlockMechanism(string characterName, string mechanismName)
        {
            if (string.IsNullOrEmpty(characterName) || string.IsNullOrEmpty(mechanismName)) return;

            if (!CurrentSave.UnlockedMechanisms.ContainsKey(characterName))
            {
                CurrentSave.UnlockedMechanisms[characterName] = new List<string>();
            }

            if (!CurrentSave.UnlockedMechanisms[characterName].Contains(mechanismName))
            {
                CurrentSave.UnlockedMechanisms[characterName].Add(mechanismName);
                Save();
                GameState.Instance.AddLog($"【機制解鎖】{characterName} 已解鎖全新能力：{mechanismName}");
            }
        }

        public static bool IsMechanismUnlocked(string characterName, string mechanismName)
        {
            if (string.IsNullOrEmpty(characterName) || string.IsNullOrEmpty(mechanismName)) return false;

            if (CurrentSave.UnlockedMechanisms.TryGetValue(characterName, out var list))
            {
                return list.Contains(mechanismName);
            }
            return false;
        }

        public static void ClearMechanismUnlocks(string characterName)
        {
            if (string.IsNullOrEmpty(characterName)) return;

            if (CurrentSave.UnlockedMechanisms.ContainsKey(characterName))
            {
                CurrentSave.UnlockedMechanisms.Remove(characterName);
                Save();
            }
        }
    }
}
