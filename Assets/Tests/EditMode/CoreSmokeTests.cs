using NUnit.Framework;
using Rugtopia.Core;

namespace Rugtopia.EditModeTests
{
    /// <summary>Smoke test so GameCI EditMode has something to run.
    /// The full rule/parity suite is CoreTests (dotnet test), not here.</summary>
    public class CoreSmokeTests
    {
        [Test]
        public void Mulberry32_IsDeterministic()
        {
            var a = new Rng(12345);
            var b = new Rng(12345);
            Assert.AreEqual(a.Next(), b.Next(), 1e-12);
            Assert.AreNotEqual(new Rng(1).Next(), new Rng(2).Next());
        }
    }
}
