using System.Linq;
using Rugtopia.Core;
using Xunit;

namespace Rugtopia.Core.Tests
{
    public class RuleTests
    {
        static GameState M(string[] rows, int belt, params string[] lanes) { return TestData.Make(rows, belt, lanes); }

        [Fact]
        public void Shuttle_path_is_boustrophedon_from_the_bottom()
        {
            var g = new LevelGrid(new[] { "AB", "CD", "EF" }, "");
            Assert.Equal("EFDCAB", new string(g.PathColor));
            Assert.Equal(new[] { 2, 2, 1, 1, 0, 0 }, g.PathRow);
            Assert.Equal(new[] { 0, 1, 1, 0, 0, 1 }, g.PathCol);
            Assert.Equal(1, g.DirectionOfRow(2)); Assert.Equal(-1, g.DirectionOfRow(1));
        }

        [Fact]
        public void Only_the_tip_colour_fires_and_waiting_spools_keep_their_slot()
        {
            var g = M(new[] { "RR", "BR" }, 3, "R3", "B1");   // path: B R | R R
            Assert.True(g.Take(0));
            Assert.Equal(0, g.Hp); Assert.Equal("R3", g.BeltKey());
            Assert.True(g.Take(1));
            Assert.Equal(GameStatus.Won, g.Status); Assert.Equal(4, g.Hp);
        }

        [Fact]
        public void Oldest_same_colour_spool_fires_first()
        {
            var g = M(new[] { "BRR" }, 4, "R1", "R1", "B1");
            g.Take(0); g.Take(1);
            g.Take(2);
            var rWeaves = g.Events.Where(e => e.Kind == EventKind.StitchWoven && e.PathIndex >= 1).Select(e => e.SpoolId).ToArray();
            Assert.Equal(new[] { 0, 1 }, rWeaves);
            Assert.Equal(GameStatus.Won, g.Status);
        }

        [Fact]
        public void A_spool_fires_only_while_its_colour_runs_then_waits()
        {
            var g = M(new[] { "AAAA", "AABB" }, 3, "A6", "B2"); // path: A A B B | A A A A
            g.Take(0);
            Assert.Equal(2, g.Hp); Assert.Equal("A4", g.BeltKey());
            g.Take(1);
            Assert.Equal(GameStatus.Won, g.Status);
        }

        [Fact]
        public void Knot_blocks_until_one_more_row_is_finished()
        {
            var g = M(new[] { "AA", "BB" }, 3, "A2k", "B2"); // path: B B | A A
            g.Take(0);
            Assert.Equal("A2k1", g.BeltKey());
            g.Take(1);
            Assert.Contains(g.Events, e => e.Kind == EventKind.RowCompleted && e.Row == 1);
            Assert.Contains(g.Events, e => e.Kind == EventKind.KnotLoosened);
            Assert.Equal(GameStatus.Won, g.Status);
        }

        [Fact]
        public void Knotted_spool_on_the_tip_colour_does_not_fire()
        {
            var g = M(new[] { "BB", "AA" }, 2, "A2k", "B2");  // path: A A | B B
            g.Take(0); g.Take(1);
            Assert.Equal(0, g.Hp);
            Assert.Equal(GameStatus.Failed, g.Status);
        }

        [Fact]
        public void Belt_full_fails_and_continue_adds_a_slot()
        {
            var g = M(new[] { "AB" }, 1, "B1,A1");
            g.Take(0);
            Assert.Equal(GameStatus.Failed, g.Status);
            Assert.False(g.Take(0));
            Assert.True(g.Continue());
            Assert.Equal(2, g.BeltSlots); Assert.Equal(1, g.Continues); Assert.Equal(GameStatus.Playing, g.Status);
            g.Take(0);
            Assert.Equal(GameStatus.Won, g.Status);
        }

        [Fact]
        public void Running_out_of_spools_without_a_match_fails()
        {
            var g = M(new[] { "AB" }, 3, "B1", "C1");
            g.Take(0); g.Take(1);
            Assert.Equal(GameStatus.Failed, g.Status);
        }

        [Fact]
        public void Slot_booster_adds_a_belt_slot()
        {
            var g = M(new[] { "AB" }, 1, "B1,A1");
            g.AddSlot();
            Assert.Equal(2, g.BeltSlots);
            g.Take(0); g.Take(0);
            Assert.Equal(GameStatus.Won, g.Status); Assert.Equal(1, g.BoostersUsed);
        }

        [Fact]
        public void Comb_picks_a_deeper_spool_once_and_is_consumed()
        {
            var g = M(new[] { "ABC" }, 2, "C1,B1,A1");
            Assert.True(g.Take(0, 2));             // no comb: depth ignored, takes front C
            Assert.Equal("C1", g.BeltKey());
            g.ToggleComb();
            Assert.True(g.CombArmed);
            Assert.True(g.Take(0, 9));             // clamped to last spool (A)
            Assert.False(g.CombArmed);
            Assert.Equal(GameStatus.Playing, g.Status);
            Assert.Equal(1, g.Hp);
            g.Take(0);
            Assert.Equal(GameStatus.Won, g.Status);
        }

        [Fact]
        public void Hidden_spool_is_revealed_when_it_reaches_the_front()
        {
            var g = M(new[] { "ABC" }, 3, "A1,?B1,?C1");
            Assert.True(g.Lanes[0][1].Hidden);
            g.Take(0);
            Assert.False(g.Lanes[0][0].Hidden);
            Assert.Contains(g.Events, e => e.Kind == EventKind.SpoolRevealed && e.Lane == 0);
        }

        [Fact]
        public void Shuttle_booster_weaves_five_stitches_and_spends_the_matching_threads()
        {
            var g = M(new[] { "CCCCCC", "AAAAAB" }, 2, "A5", "B1", "C6");
            Assert.True(g.UseShuttle());
            Assert.Equal(5, g.Hp);
            Assert.Empty(g.Lanes[0]);
            g.Take(1); g.Take(2);
            Assert.Equal(GameStatus.Won, g.Status);
        }

        [Fact]
        public void Shuttle_booster_stops_at_the_end_of_the_active_row()
        {
            var g = M(new[] { "CC", "AA" }, 2, "A2", "C2");
            g.UseShuttle();
            Assert.Equal(2, g.Hp); Assert.Equal(0, g.ActiveRow);
            Assert.Contains(g.Events, e => e.Kind == EventKind.RowCompleted);
        }

        [Fact]
        public void Covered_rows_are_veiled_until_active()
        {
            var d = new LevelData { rows = new[] { "AA", "AA", "AA", "AA" }, covered = "top", belt = 2, lanes = 1, cap = 9 };
            var g = new GameState(d, new System.Collections.Generic.List<System.Collections.Generic.List<Spool>> { new System.Collections.Generic.List<Spool> { new Spool { Color = 'A', Left = 8, Total = 8 } } });
            Assert.False(g.IsRowVisible(0)); Assert.False(g.IsRowVisible(1)); Assert.True(g.IsRowVisible(2));
            Assert.True(LevelGrid.IsCovered("alt", 4, 2)); Assert.False(LevelGrid.IsCovered("alt", 4, 3));
        }

        [Fact]
        public void Preview_strip_stops_at_a_covered_row()
        {
            var d = new LevelData { rows = new[] { "BB", "AB" }, covered = "top", belt = 2, lanes = 1, cap = 9 };
            var g = new GameState(d, new System.Collections.Generic.List<System.Collections.Generic.List<Spool>> { new System.Collections.Generic.List<Spool> { new Spool { Color = 'A', Left = 1 } } });
            var runs = g.PreviewRuns();
            Assert.Equal('A', runs[0].Key); Assert.Equal('B', runs[1].Key); Assert.Equal('?', runs[2].Key);
        }

        [Fact]
        public void Clone_is_independent()
        {
            var g = M(new[] { "AB" }, 2, "B1,A1");
            var c = g.Clone();
            c.Take(0);
            Assert.Equal(2, g.Lanes[0].Count); Assert.Equal(1, c.Lanes[0].Count);
        }

        [Fact]
        public void Solver_reports_unsolvable_levels()
        {
            var r = Solver.Solve(M(new[] { "AB" }, 1, "B1,A1"));
            Assert.False(r.Ok); Assert.Null(r.Path);
        }

        [Fact]
        public void Economy_constants_match_prototype()
        {
            Assert.Equal(40, EconomyConfig.Reward("ftue")); Assert.Equal(150, EconomyConfig.Reward("boss"));
            Assert.Equal(900, EconomyConfig.ContinuePrice(0)); Assert.Equal(2100, EconomyConfig.ContinuePrice(7));
            Assert.False(EconomyConfig.LosesLifeOnFail(14)); Assert.True(EconomyConfig.LosesLifeOnFail(15));
            Assert.Equal(7, EconomyConfig.UnlockSlot); Assert.Equal(13, EconomyConfig.UnlockComb); Assert.Equal(17, EconomyConfig.UnlockShuttle);
            Assert.Equal(29, EconomyConfig.UnlockInterstitial);
        }
    }
}
