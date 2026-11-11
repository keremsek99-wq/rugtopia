using System;
using System.Collections.Generic;

namespace Rugtopia.Core
{
    /// <summary>Port of buildLevel() in game.js (weft order). Deals spools into lanes with the same seeded RNG.</summary>
    public static class LevelBuilder
    {
        public static GameState Build(LevelData d) { return new GameState(d, Deal(d)); }

        /// <summary>Build from the lanes stored in the JSON (exactly what the prototype dealt).</summary>
        public static GameState BuildFromGenerated(LevelData d)
        {
            var lanes = new List<List<Spool>>();
            foreach (var ld in d.generated.lanes)
            {
                var l = new List<Spool>();
                foreach (var s in ld.spools) l.Add(new Spool { Id = s.seq, Seq = s.seq, Color = s.c[0], Left = s.n, Total = s.n, Hidden = s.hidden, Knot = s.knot });
                lanes.Add(l);
            }
            return new GameState(d, lanes);
        }

        public static List<List<Spool>> Deal(LevelData d)
        {
            var grid = new LevelGrid(d.rows, d.covered);
            // 1) thread sequence along the shuttle path; a spool closes when it reaches cap
            var seq = new List<Spool>();
            var open = new Dictionary<char, Spool>();
            for (int i = 0; i < grid.Length; i++)
            {
                char col = grid.PathColor[i];
                Spool s;
                if (!open.TryGetValue(col, out s)) { s = new Spool { Color = col }; open[col] = s; seq.Add(s); }
                s.Left++;
                if (s.Left >= d.cap) open.Remove(col);
            }
            // 2) deal into lanes
            var rand = new Rng(1000 + d.index * 7919 + d.seed * 104729);
            var lanes = new List<List<Spool>>();
            for (int i = 0; i < d.lanes; i++) lanes.Add(new List<Spool>());
            for (int i = 0; i < seq.Count; i++)
            {
                int lane = rand.Next() < d.shuffle ? (int)Math.Floor(rand.Next() * d.lanes) : i % d.lanes;
                var s = seq[i];
                s.Seq = i; s.Id = i; s.Total = s.Left; s.Hidden = rand.Next() < d.hidden; s.Knot = 0;
                lanes[lane].Add(s);
            }
            // 3) knots
            var kr = new Rng(77 + d.index * 31 + d.seed);
            int placed = 0, guard = 0;
            while (placed < d.knots && guard++ < 500 && seq.Count > 3)
            {
                int k = 2 + (int)Math.Floor(kr.Next() * (seq.Count - 2));
                if (seq[k].Knot == 0) { seq[k].Knot = 1; placed++; }
            }
            foreach (var l in lanes) if (l.Count > 0) l[0].Hidden = false;
            return lanes;
        }
    }
}
