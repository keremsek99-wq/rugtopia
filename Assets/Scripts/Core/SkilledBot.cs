using System;
using System.Collections.Generic;

namespace Rugtopia.Core
{
    /// <summary>Skilled-player bot used for difficulty tuning (port of botSkilledRun/mEval/mLook in game.js v3.1).
    /// Reads the preview strip (12 stitches, never into a covered row), looks 2 taps ahead, and slips randomly with probability eps.</summary>
    public static class SkilledBot
    {
        public const int Preview = 12;
        public const int Depth = 2;
        public const double DefaultEps = 0.15;

        public struct RunResult { public bool Win; public int Taps; public int Hp; }
        public struct Stats { public double Fail; public double Taps; }

        public static List<char> Visible(GameState m)
        {
            var o = new List<char>();
            int act = m.ActiveRow; if (act < 0) return o;
            for (int k = m.Hp; k < m.Grid.Length && o.Count < Preview; k++)
            {
                int r = m.Grid.PathRow[k];
                if (r != act && m.Grid.CoveredRow[r]) break;
                o.Add(m.Grid.PathColor[k]);
            }
            return o;
        }

        public static double Eval(GameState m)
        {
            if (m.Status == GameStatus.Won) return 1e5;
            if (m.Status == GameStatus.Failed) return -1e6;
            var vis = Visible(m);
            double e = m.Hp * 3 + (m.BeltSlots - m.Belt.Count) * 2;
            foreach (var s in m.Belt)
            {
                int ix = vis.IndexOf(s.Color);
                e += ix >= 0 ? (Preview - ix) / 4.0 : -4;
                if (s.Knot > 0) e -= 1.5;
            }
            return e;
        }

        public static double Look(GameState m, int depth)
        {
            double e = Eval(m);
            if (depth <= 0 || m.Status == GameStatus.Won || e <= -1e5) return e;
            double best = -1e7;
            for (int i = 0; i < m.Lanes.Count; i++)
            {
                if (m.Lanes[i].Count == 0) continue;
                var c = m.Clone(); c.Take(i);
                double v = Look(c, depth - 1); if (v > best) best = v;
            }
            return best == -1e7 ? e : Math.Max(best, e > -1e5 && m.Belt.Count < m.BeltSlots ? -1e5 : best);
        }

        public static RunResult Run(GameState start, Rng rand, double eps)
        {
            var m = start.Clone(); int taps = 0;
            while (m.Status != GameStatus.Won)
            {
                if (m.Status == GameStatus.Failed) return new RunResult { Win = false, Taps = taps, Hp = m.Hp };
                var ci = new List<int>(); var cs = new List<double>();
                for (int i = 0; i < m.Lanes.Count; i++)
                {
                    if (m.Lanes[i].Count == 0) continue;
                    var c = m.Clone(); c.Take(i);
                    ci.Add(i); cs.Add(Look(c, Depth - 1));
                }
                var alive = new List<int>();
                for (int k = 0; k < ci.Count; k++) if (cs[k] > -1e5) alive.Add(k);
                int pick;
                if (alive.Count > 0 && rand.Next() < eps) pick = alive[(int)Math.Floor(rand.Next() * alive.Count)];
                else
                {
                    double best = double.NegativeInfinity; foreach (var v in cs) if (v > best) best = v;
                    var top = new List<int>(); for (int k = 0; k < ci.Count; k++) if (cs[k] >= best - 0.01) top.Add(k);
                    pick = top[(int)Math.Floor(rand.Next() * top.Count)];
                }
                m.Take(ci[pick]); taps++;
            }
            return new RunResult { Win = true, Taps = taps, Hp = m.Hp };
        }

        /// <summary>Same seeding as botSkilledStats(idx) in the prototype: rng(9191 + idx).</summary>
        public static Stats Measure(LevelData level, int runs, double eps = DefaultEps)
        {
            var m0 = LevelBuilder.Build(level); var rand = new Rng(9191 + level.index);
            int f = 0, w = 0, taps = 0;
            for (int k = 0; k < runs; k++) { var r = Run(m0, rand, eps); if (r.Win) { w++; taps += r.Taps; } else f++; }
            return new Stats { Fail = (double)f / runs, Taps = w > 0 ? (double)taps / w : 0 };
        }
    }
}
