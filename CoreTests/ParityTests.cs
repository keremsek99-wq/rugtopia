using System.Collections.Generic;
using System.Linq;
using Rugtopia.Core;
using Xunit;

namespace Rugtopia.Core.Tests
{
    /// <summary>C# core must reproduce the HTML5 prototype (game.js v3.1) exactly. Fixtures come from Tools/export_levels.js.</summary>
    public class ParityTests
    {
        [Fact]
        public void Rng_matches_js_mulberry32_bit_for_bit()
        {
            Assert.NotEmpty(TestData.Parity.rng);
            foreach (var s in TestData.Parity.rng)
            {
                var r = new Rng(s.seed);
                foreach (var v in s.values) Assert.Equal(v, r.Next());
            }
        }

        [Theory, MemberData(nameof(TestData.LevelIndices), MemberType = typeof(TestData))]
        public void Deal_matches_js_buildLevel(int i)
        {
            var d = TestData.Levels[i];
            var lanes = LevelBuilder.Deal(d);
            Assert.Equal(d.generated.lanes.Count, lanes.Count);
            for (int l = 0; l < lanes.Count; l++)
            {
                var js = d.generated.lanes[l].spools;
                Assert.Equal(js.Count, lanes[l].Count);
                for (int k = 0; k < js.Count; k++)
                {
                    var a = lanes[l][k]; var b = js[k];
                    Assert.Equal(b.c[0], a.Color); Assert.Equal(b.n, a.Left); Assert.Equal(b.hidden, a.Hidden);
                    Assert.Equal(b.knot, a.Knot); Assert.Equal(b.seq, a.Seq);
                }
            }
        }

        [Theory, MemberData(nameof(TestData.LevelIndices), MemberType = typeof(TestData))]
        public void Move_sequences_match_js_model_step_by_step(int i)
        {
            var pl = TestData.Parity.levels[i];
            Assert.Equal(3, pl.sequences.Count);
            foreach (var sq in pl.sequences)
            {
                var g = LevelBuilder.Build(TestData.Levels[i]);
                Check(g, sq.steps[0], sq.name, 0);
                for (int k = 0; k < sq.moves.Length; k++)
                {
                    Assert.True(g.Take(sq.moves[k]), sq.name + " step " + k + " take refused");
                    Check(g, sq.steps[k + 1], sq.name, k + 1);
                }
            }
        }

        [Theory, MemberData(nameof(TestData.LevelIndices), MemberType = typeof(TestData))]
        public void Solver_finds_the_same_path_with_the_same_node_count(int i)
        {
            var r = Solver.Solve(LevelBuilder.Build(TestData.Levels[i]));
            Assert.True(r.Ok);
            Assert.Equal(TestData.Levels[i].solution, r.Path.ToArray());
            Assert.Equal(TestData.Parity.levels[i].solverNodes, r.Nodes);
        }

        [Theory, MemberData(nameof(TestData.LevelIndices), MemberType = typeof(TestData))]
        public void Skilled_bot_gives_the_same_fail_rate_as_js(int i)
        {
            var pl = TestData.Parity.levels[i];
            var st = SkilledBot.Measure(TestData.Levels[i], pl.botRuns);
            Assert.Equal(pl.botFail, st.Fail, 9);
            Assert.Equal(pl.botTaps, st.Taps, 9);
        }

        static void Check(GameState g, ParityStep s, string seq, int step)
        {
            string at = seq + " step " + step;
            Assert.True(s.hp == g.Hp, at + ": hp " + g.Hp + " vs js " + s.hp);
            Assert.True(s.active == g.ActiveRow, at + ": active " + g.ActiveRow + " vs js " + s.active);
            Assert.True(s.belt == g.BeltKey(), at + ": belt '" + g.BeltKey() + "' vs js '" + s.belt + "'");
            Assert.True(s.lanes == g.LanesKey(), at + ": lanes " + g.LanesKey() + " vs js " + s.lanes);
            Assert.True(s.dead == (g.Status == GameStatus.Failed), at + ": dead");
            Assert.True(s.won == (g.Status == GameStatus.Won), at + ": won");
        }
    }
}
