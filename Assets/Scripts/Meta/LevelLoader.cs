using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.Meta
{
    /// <summary>Loads the 30 exported levels. Runtime copy: Assets/Resources/levels.json (written by Tools/export_levels.js);
    /// per-level files in Assets/Data/Levels are the editable/reviewable source and are used by the dotnet tests.
    /// UNVERIFIED in Unity: JsonUtility parsing of LevelPack (verified with System.Text.Json in CoreTests only).</summary>
    public static class LevelLoader
    {
        private static LevelPack _pack;

        public static LevelPack Pack
        {
            get
            {
                if (_pack != null) return _pack;
                var ta = Resources.Load<TextAsset>("levels");
                if (ta == null) { Debug.LogError("Rugtopia: Resources/levels.json missing. Run: node Tools/export_levels.js"); _pack = new LevelPack(); return _pack; }
                _pack = JsonUtility.FromJson<LevelPack>(ta.text);
                return _pack;
            }
        }

        public static int Count { get { return Pack.levels.Count; } }
        public static LevelData Get(int index) { return Pack.levels[Mathf.Clamp(index, 0, Count - 1)]; }

        /// <summary>Deals with the C# LevelBuilder (bit-identical to the prototype, see CoreTests ParityTests).</summary>
        public static GameState NewGame(int index) { return LevelBuilder.Build(Get(index)); }
    }
}
