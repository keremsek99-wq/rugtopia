using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Rugtopia.Core;

namespace Rugtopia.Core.Tests
{
    public class ParityStep { public int hp; public int active; public string belt; public string lanes; public bool dead; public bool won; }
    public class ParitySeq { public string name; public int[] moves; public List<ParityStep> steps; }
    public class ParityLevel { public int index; public int solverNodes; public int botRuns; public double botFail; public double botTaps; public List<ParitySeq> sequences; }
    public class RngSample { public int seed; public double[] values; }
    public class ParityFile { public string source; public List<RngSample> rng; public List<ParityLevel> levels; }

    public static class TestData
    {
        static readonly JsonSerializerOptions Opt = new JsonSerializerOptions { IncludeFields = true, PropertyNameCaseInsensitive = true };
        static string Root
        {
            get
            {
                var d = new DirectoryInfo(AppContext.BaseDirectory);
                while (d != null && !Directory.Exists(Path.Combine(d.FullName, "Assets"))) d = d.Parent;
                if (d == null) throw new DirectoryNotFoundException("project root with Assets/ not found");
                return d.FullName;
            }
        }
        // Lazy<T> is thread-safe: xUnit runs test classes in parallel.
        static readonly Lazy<List<LevelData>> _levels = new Lazy<List<LevelData>>(() =>
        {
            var dir = Path.Combine(Root, "Assets", "Data", "Levels");
            var files = Directory.GetFiles(dir, "level_*.json"); Array.Sort(files, StringComparer.Ordinal);
            var list = new List<LevelData>();
            foreach (var f in files) list.Add(JsonSerializer.Deserialize<LevelData>(File.ReadAllText(f), Opt));
            return list;
        });
        public static List<LevelData> Levels { get { return _levels.Value; } }
        public static LevelPack Pack { get { return JsonSerializer.Deserialize<LevelPack>(File.ReadAllText(Path.Combine(Root, "Assets", "Resources", "levels.json")), Opt); } }
        static readonly Lazy<ParityFile> _parity = new Lazy<ParityFile>(() => JsonSerializer.Deserialize<ParityFile>(File.ReadAllText(Path.Combine(Root, "CoreTests", "Fixtures", "parity.json")), Opt));
        public static ParityFile Parity { get { return _parity.Value; } }
        public static IEnumerable<object[]> LevelIndices() { for (int i = 0; i < 30; i++) yield return new object[] { i }; }

        /// <summary>Tiny hand-made level for rule tests. Lanes given as "R3,B2" per lane (front first, k suffix = knot).</summary>
        public static GameState Make(string[] rows, int belt, params string[] lanes)
        {
            var d = new LevelData { rows = rows, belt = belt, lanes = lanes.Length, cap = 9, covered = "", tag = "normal" };
            var ls = new List<List<Spool>>(); int id = 0;
            foreach (var lane in lanes)
            {
                var l = new List<Spool>();
                if (lane.Length > 0)
                    foreach (var tok in lane.Split(','))
                    {
                        var t = tok.Trim(); bool hid = t.StartsWith("?"); if (hid) t = t.Substring(1);
                        int knot = t.EndsWith("k") ? 1 : 0; if (knot == 1) t = t.Substring(0, t.Length - 1);
                        int n = int.Parse(t.Substring(1));
                        l.Add(new Spool { Id = id, Seq = id, Color = t[0], Left = n, Total = n, Knot = knot, Hidden = hid && l.Count > 0 }); id++;
                    }
                ls.Add(l);
            }
            return new GameState(d, ls);
        }
    }
}
