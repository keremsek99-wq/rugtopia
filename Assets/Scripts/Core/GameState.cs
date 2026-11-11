using System;
using System.Collections.Generic;
using System.Text;

namespace Rugtopia.Core
{
    /// <summary>
    /// Engine-independent Rugtopia rules (v3.1, "Atkı Yolu" / Weft Path).
    /// - The shuttle weaves the rug bottom-up, one stitch at a time, boustrophedon (bottom row L->R, next row R->L, ...).
    /// - Only the stitch at the shuttle tip (path index Hp) can be woven; a belt spool of that colour fires (oldest first).
    /// - Other spools wait on the belt and keep their slot. Empty spools leave immediately.
    /// - A knotted spool on the belt fires only after one more row is finished (knot loosens on each row completion).
    /// - Fail: belt full (or no spools left) and nothing can fire. Win: every stitch woven.
    /// Woven state is implicit: path index &lt; Hp is woven.
    /// Real-time pacing (0.11 s per stitch, flight animation) is a view concern; the Core resolves weaving instantly.
    /// </summary>
    public sealed class GameState
    {
        public const int MaxCombDepth = 3;   // Comb: the 4 visible spools of a lane can be picked (prototype draws 4)
        public const int ShuttleStitches = 5;

        public readonly LevelData Level;
        public readonly LevelGrid Grid;
        public List<List<Spool>> Lanes { get; private set; }
        public List<Spool> Belt { get; private set; }  // ordered by take order (oldest first)
        public int BeltSlots { get; private set; }
        public int Hp { get; private set; }          // shuttle tip index into Grid path
        public GameStatus Status { get; private set; }
        public int Continues { get; private set; }
        public bool CombArmed { get; private set; }   // next Take may pick deeper than the front (consumed by that Take)
        public int Taps { get; private set; }
        public int BoostersUsed { get; private set; }
        private int _orderCounter;
        public readonly List<GameEvent> Events = new List<GameEvent>();

        public GameState(LevelData level, List<List<Spool>> lanes)
        {
            Level = level;
            Grid = new LevelGrid(level.rows, level.covered);
            Lanes = lanes;
            Belt = new List<Spool>();
            BeltSlots = level.belt;
            Status = GameStatus.Playing;
            Settle();
            Events.Clear();
        }

        private GameState(GameState o)
        {
            Level = o.Level; Grid = o.Grid;
            Lanes = new List<List<Spool>>(o.Lanes.Count);
            foreach (var l in o.Lanes) { var nl = new List<Spool>(l.Count); foreach (var s in l) nl.Add(s.Clone()); Lanes.Add(nl); }
            Belt = new List<Spool>(o.Belt.Count);
            foreach (var s in o.Belt) Belt.Add(s.Clone());
            BeltSlots = o.BeltSlots; Hp = o.Hp; Status = o.Status; Continues = o.Continues; CombArmed = o.CombArmed;
            Taps = o.Taps; BoostersUsed = o.BoostersUsed; _orderCounter = o._orderCounter;
        }
        public GameState Clone() { return new GameState(this); }

        // ---------- queries ----------
        public int ActiveRow { get { return Grid.RowOf(Hp); } }
        public bool IsWoven(int r, int c) { for (int i = 0; i < Hp; i++) if (Grid.PathRow[i] == r && Grid.PathCol[i] == c) return true; return false; }
        public bool IsWovenPath(int pathIndex) { return pathIndex < Hp; }
        /// <summary>Covered rows stay veiled until they become active.</summary>
        public bool IsRowVisible(int r) { return !Grid.CoveredRow[r] || r >= ActiveRow; }
        public char? TipColor { get { return Hp < Grid.Length ? Grid.PathColor[Hp] : (char?)null; } }
        public int FreeSlots { get { return BeltSlots - Belt.Count; } }
        public int SpoolsLeftInLanes { get { int n = 0; foreach (var l in Lanes) n += l.Count; return n; } }
        public double Progress { get { return (double)Hp / Grid.Length; } }
        public int StitchesLeft { get { return Grid.Length - Hp; } }

        /// <summary>Next colour runs the shuttle needs, for the NEXT strip (never reveals a still-covered row).</summary>
        public List<KeyValuePair<char, int>> PreviewRuns(int maxRows = 2)
        {
            var runs = new List<KeyValuePair<char, int>>();
            int act = ActiveRow; if (act < 0) return runs;
            int lim = Math.Min(Grid.Length, (Hp / Grid.Cols + maxRows) * Grid.Cols);
            int lastRow = -1;
            for (int k = Hp; k < lim; k++)
            {
                int r = Grid.PathRow[k];
                if (r != act && Grid.CoveredRow[r]) { runs.Add(new KeyValuePair<char, int>('?', 0)); break; }
                char col = Grid.PathColor[k];
                if (runs.Count > 0 && runs[runs.Count - 1].Key == col && lastRow == r) runs[runs.Count - 1] = new KeyValuePair<char, int>(col, runs[runs.Count - 1].Value + 1);
                else { if (lastRow != -1 && lastRow != r) runs.Add(new KeyValuePair<char, int>('|', 0)); runs.Add(new KeyValuePair<char, int>(col, 1)); }
                lastRow = r;
            }
            return runs;
        }

        public bool CanTake(int lane, int depth = 0)
        {
            if (Status != GameStatus.Playing || lane < 0 || lane >= Lanes.Count) return false;
            var l = Lanes[lane];
            if (l.Count == 0 || FreeSlots <= 0) return false;
            return true; // depth is ignored unless comb mode is armed (as in the prototype)
        }

        // ---------- actions ----------
        /// <summary>Tap a lane: its front spool (or, with a Comb charge, the spool at <paramref name="depth"/>) goes to the belt.</summary>
        public bool Take(int lane, int depth = 0)
        {
            Events.Clear();
            if (!CanTake(lane, depth)) return false;
            var l = Lanes[lane];
            if (CombArmed) { depth = Math.Max(0, Math.Min(Math.Min(depth, MaxCombDepth), l.Count - 1)); CombArmed = false; BoostersUsed++; }
            else depth = 0;
            var s = l[depth];
            l.RemoveAt(depth);
            s.Hidden = false; s.Order = ++_orderCounter;
            Belt.Add(s);
            Taps++;
            Emit(EventKind.SpoolTaken, s.Id, lane);
            if (l.Count > 0 && l[0].Hidden) { l[0].Hidden = false; Emit(EventKind.SpoolRevealed, l[0].Id, lane); }
            Settle();
            return true;
        }

        /// <summary>+1 Slot booster.</summary>
        public void AddSlot()
        {
            Events.Clear();
            if (Status != GameStatus.Playing) return;
            BeltSlots++; BoostersUsed++;
            Emit(EventKind.SlotAdded, -1, -1);
        }

        /// <summary>Comb booster: toggles comb mode; the next Take may pick any of the top 4 spools of a lane.
        /// (Paying for it is a Meta/Economy concern, done when the comb Take happens, as in the prototype.)</summary>
        public void ToggleComb() { if (Status == GameStatus.Playing) CombArmed = !CombArmed; }

        /// <summary>Shuttle booster (v3.1): weaves the next 5 stitches of the active row along the shuttle path, whatever their colour.
        /// Threads are taken from the matching spool furthest back in the sequence (lanes first, then belt), so the thread budget stays exact.</summary>
        public bool UseShuttle()
        {
            Events.Clear();
            if (Status != GameStatus.Playing) return false;
            int act = ActiveRow; if (act < 0) return false;
            int n = 0;
            while (n < ShuttleStitches && Hp < Grid.Length && Grid.PathRow[Hp] == act)
            {
                TakeThread(Grid.PathColor[Hp]);
                Emit(EventKind.StitchWoven, -1, -1, Grid.PathRow[Hp], Grid.PathCol[Hp], Hp);
                Hp++; n++;
                if (Hp % Grid.Cols == 0) OnRowCompleted(act);
            }
            if (n == 0) return false;
            BoostersUsed++;
            Emit(EventKind.ShuttleBooster, -1, -1);
            Settle();
            return true;
        }

        /// <summary>Continue after a fail: +1 belt slot. (The free Comb that comes with it goes to the inventory, see Meta/Economy.)</summary>
        public bool Continue()
        {
            Events.Clear();
            if (Status != GameStatus.Failed) return false;
            Continues++; BeltSlots++;
            Status = GameStatus.Playing;
            Emit(EventKind.Continued, -1, -1);
            Settle();
            return true;
        }

        // ---------- rules ----------
        private void TakeThread(char color)
        {
            Spool best = null; List<Spool> where = null;
            foreach (var l in Lanes) foreach (var s in l) if (s.Color == color && s.Left > 0 && (best == null || s.Seq > best.Seq)) { best = s; where = l; }
            if (best == null) { foreach (var s in Belt) if (s.Color == color && s.Left > 0 && (best == null || s.Seq > best.Seq)) { best = s; where = Belt; } }
            if (best == null) return;
            best.Left--; best.Total = Math.Max(best.Total - 1, best.Left);
            if (best.Left <= 0)
            {
                where.Remove(best);
                Emit(EventKind.SpoolEmptied, best.Id, -1);
                if (where != Belt) { int li = Lanes.IndexOf(where); if (where.Count > 0 && where[0].Hidden) { where[0].Hidden = false; Emit(EventKind.SpoolRevealed, where[0].Id, li); } }
            }
        }

        private void OnRowCompleted(int row)
        {
            Emit(EventKind.RowCompleted, -1, -1, row);
            foreach (var s in Belt) if (s.Knot > 0) { s.Knot--; Emit(EventKind.KnotLoosened, s.Id, -1); }
        }

        /// <summary>Weave as far as the belt allows (same result as the prototype's real-time firing).</summary>
        private void Settle()
        {
            if (Status != GameStatus.Playing) return;
            while (Hp < Grid.Length)
            {
                char h = Grid.PathColor[Hp];
                Spool s = null;
                foreach (var b in Belt) if (b.Knot == 0 && b.Left > 0 && b.Color == h) { s = b; break; }
                if (s == null) break;
                while (s.Left > 0 && Hp < Grid.Length && Grid.PathColor[Hp] == h)
                {
                    int row = Grid.PathRow[Hp];
                    Emit(EventKind.StitchWoven, s.Id, -1, row, Grid.PathCol[Hp], Hp);
                    s.Left--; Hp++;
                    if (Hp % Grid.Cols == 0) OnRowCompleted(row);
                }
                if (s.Left <= 0) { Belt.Remove(s); Emit(EventKind.SpoolEmptied, s.Id, -1); }
            }
            if (Hp >= Grid.Length) { Status = GameStatus.Won; Emit(EventKind.Won, -1, -1); return; }
            if (Belt.Count >= BeltSlots || SpoolsLeftInLanes == 0) { Status = GameStatus.Failed; Emit(EventKind.Failed, -1, -1); }
        }

        private void Emit(EventKind k, int spool, int lane, int row = -1, int col = -1, int pathIndex = -1)
        {
            Events.Add(new GameEvent { Kind = k, SpoolId = spool, Lane = lane, Row = row, Col = col, PathIndex = pathIndex });
        }

        // ---------- debug / parity ----------
        public string BeltKey()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Belt.Count; i++) { if (i > 0) sb.Append(','); sb.Append(Belt[i]); }
            return sb.ToString();
        }
        public string LanesKey()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Lanes.Count; i++) { if (i > 0) sb.Append(','); sb.Append(Lanes[i].Count); }
            return sb.ToString();
        }
        /// <summary>Canonical state key (equivalent to mKey in game.js).</summary>
        public string StateKey() { return LanesKey() + "|" + BeltKey() + "|" + Hp + "|" + BeltSlots; }
    }
}
