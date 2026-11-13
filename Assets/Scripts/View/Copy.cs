using System.Collections.Generic;
using Rugtopia.Meta;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Reads Assets/Resources/UI_strings.csv (same table as Assets/Localization/UI_strings.csv).</summary>
    public static class Copy
    {
        static Dictionary<string, string[]> _rows;

        public static string Get(string key)
        {
            Ensure();
            string lang = "en";
            if (SaveService.Data != null && SaveService.Data.lang == "tr") lang = "tr";
            if (_rows != null && _rows.TryGetValue(key, out var pair))
                return lang == "tr" ? pair[1] : pair[0];
            return key;
        }

        static void Ensure()
        {
            if (_rows != null) return;
            _rows = new Dictionary<string, string[]>();
            var ta = Resources.Load<TextAsset>("UI_strings");
            if (ta == null) return;
            var lines = ta.text.Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = Split(lines[i].Trim());
                if (parts.Count >= 3) _rows[parts[0]] = new[] { parts[1], parts[2] };
            }
        }

        static List<string> Split(string line)
        {
            var parts = new List<string>();
            if (line.Length == 0) return parts;
            bool q = false;
            var cur = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"') { q = !q; continue; }
                if (c == ',' && !q) { parts.Add(cur.ToString()); cur.Length = 0; continue; }
                cur.Append(c);
            }
            parts.Add(cur.ToString());
            return parts;
        }
    }
}
