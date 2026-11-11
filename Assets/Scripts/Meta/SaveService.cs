using System;
using System.Collections.Generic;
using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.Meta
{
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public string lang = "en";
        public int maxLevel;            // highest unlocked level index (0-based)
        public int coins = EconomyConfig.StartCoins;
        public int invSlot, invComb, invShuttle;
        public int lives = EconomyConfig.MaxLives;
        public long livesTicksUtc;      // DateTime.UtcNow.Ticks when the refill timer started
        public int streak;
        public List<string> seenIntros = new List<string>();
        public List<float> bestSeconds = new List<float>();
        public bool pickedLang;
        public string theme = "light";
        public string freeContinueDay = "";
        public long firstPlayUtc;
    }

    /// <summary>PlayerPrefs JSON save (week-1 stub; cloud save / iCloud is out of slice scope).</summary>
    public static class SaveService
    {
        public const string Key = "rugtopia_save_v1";
        private static SaveData _data;

        public static SaveData Data { get { if (_data == null) Load(); return _data; } }

        public static void Load()
        {
            var json = PlayerPrefs.GetString(Key, "");
            _data = string.IsNullOrEmpty(json) ? NewSave() : JsonUtility.FromJson<SaveData>(json);
            if (_data == null) _data = NewSave();
            if (_data.firstPlayUtc == 0) { _data.firstPlayUtc = DateTime.UtcNow.Ticks; Save(); }
            if (_data.seenIntros == null) _data.seenIntros = new List<string>();
            if (_data.bestSeconds == null) _data.bestSeconds = new List<float>();
        }

        public static void Save()
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        public static void Reset() { _data = NewSave(); Save(); }

        private static SaveData NewSave() { return new SaveData { livesTicksUtc = DateTime.UtcNow.Ticks }; }
    }
}
