using System.Collections.Generic;

namespace Rugtopia.Core
{
    public sealed class SolveResult
    {
        public bool Ok;
        public List<int> Path;   // lane sequence
        public int Nodes;
        public bool Exhausted;   // node limit hit
    }

    /// <summary>Depth-first search over lane take orders (port of solveModel in game.js; front-only taps, no boosters).
    /// Lanes are tried in order of how soon the shuttle needs their front colour.</summary>
    public static class Solver
    {
        public static SolveResult Solve(GameState start, int limit = 400000)
        {
            var res = new SolveResult();
            var seen = new HashSet<string>();
            var path = new List<int>();
            int nodes = 0; bool cut = false;

            bool Rec(GameState m)
            {
                if (m.Status == GameStatus.Won) return true;
                if (++nodes > limit) { cut = true; return false; }
                if (m.Status == GameStatus.Failed) return false;
                if (!seen.Add(m.StateKey())) return false;
                var idx = new List<int>();
                for (int i = 0; i < m.Lanes.Count; i++) if (m.Lanes[i].Count > 0) idx.Add(i);
                // stable sort by distance to the front colour's next need (JS Array.prototype.sort is stable)
                var dist = new Dictionary<int, int>();
                foreach (int i in idx) dist[i] = Dist(m, m.Lanes[i][0].Color);
                idx = StableSort(idx, dist);
                foreach (int i in idx)
                {
                    var c = m.Clone();
                    c.Take(i);
                    path.Add(i);
                    if (Rec(c)) return true;
                    path.RemoveAt(path.Count - 1);
                    if (cut) return false;
                }
                return false;
            }

            bool ok = Rec(start.Clone());
            res.Ok = ok; res.Path = ok ? new List<int>(path) : null; res.Nodes = nodes; res.Exhausted = cut;
            return res;
        }

        public static int Dist(GameState m, char c)
        {
            for (int k = m.Hp; k < m.Grid.Length; k++) if (m.Grid.PathColor[k] == c) return k - m.Hp;
            return 1000000;
        }

        private static List<int> StableSort(List<int> idx, Dictionary<int, int> key)
        {
            var a = new List<int>(idx);
            for (int i = 1; i < a.Count; i++) { int v = a[i], j = i - 1; while (j >= 0 && key[a[j]] > key[v]) { a[j + 1] = a[j]; j--; } a[j + 1] = v; }
            return a;
        }
    }
}
