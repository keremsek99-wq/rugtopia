using System.Collections.Generic;
using System.Linq;
using Rugtopia.Core;
using Xunit;

namespace Rugtopia.Core.Tests
{
    public class LevelTests
    {
        [Fact]
        public void There_are_30_levels_and_the_runtime_pack_matches_the_files()
        {
            Assert.Equal(30, TestData.Levels.Count);
            var pack = TestData.Pack;
            Assert.Equal(30, pack.levels.Count);
            for (int i = 0; i < 30; i++)
            {
                Assert.Equal(i, TestData.Levels[i].index);
                Assert.Equal(TestData.Levels[i].rows, pack.levels[i].rows);
                Assert.Equal(TestData.Levels[i].belt, pack.levels[i].belt);
            }
        }

        [Theory, MemberData(nameof(TestData.LevelIndices), MemberType = typeof(TestData))]
        public void Every_level_is_solvable_in_csharp(int i)
        {
            var r = Solver.Solve(LevelBuilder.Build(TestData.Levels[i]));
            Assert.True(r.Ok, "level " + (i + 1) + " unsolvable, nodes " + r.Nodes);
            Assert.False(r.Exhausted);
        }

        [Theory, MemberData(nameof(TestData.LevelIndices), MemberType = typeof(TestData))]
        public void Js_solution_replays_to_a_win_from_stored_lanes(int i)
        {
            var g = LevelBuilder.BuildFromGenerated(TestData.Levels[i]);
            foreach (var m in TestData.Levels[i].solution) Assert.True(g.Take(m));
            Assert.Equal(GameStatus.Won, g.Status);
            Assert.Equal(0, g.Belt.Count);
        }

        [Theory, MemberData(nameof(TestData.LevelIndices), MemberType = typeof(TestData))]
        public void Thread_budget_equals_stitch_count_per_colour(int i)
        {
            var d = TestData.Levels[i];
            var need = new Dictionary<char, int>();
            foreach (var row in d.rows) foreach (var ch in row) need[ch] = (need.TryGetValue(ch, out var n) ? n : 0) + 1;
            var have = new Dictionary<char, int>();
            foreach (var l in LevelBuilder.Deal(d)) foreach (var s in l) { have[s.Color] = (have.TryGetValue(s.Color, out var n) ? n : 0) + s.Left; Assert.InRange(s.Left, 1, d.cap); }
            Assert.Equal(need.OrderBy(k => k.Key), have.OrderBy(k => k.Key));
        }

        [Theory, MemberData(nameof(TestData.LevelIndices), MemberType = typeof(TestData))]
        public void Lane_fronts_are_never_hidden_and_knot_count_matches(int i)
        {
            var d = TestData.Levels[i];
            var lanes = LevelBuilder.Deal(d);
            foreach (var l in lanes) if (l.Count > 0) Assert.False(l[0].Hidden);
            int total = lanes.Sum(l => l.Count);
            Assert.Equal(total > 3 ? d.knots : 0, lanes.Sum(l => l.Count(s => s.Knot > 0)));
        }
    }
}
