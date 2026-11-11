namespace Rugtopia.Core
{
    /// <summary>mulberry32, bit-exact port of rng() in the HTML5 prototype (game.js).</summary>
    public sealed class Rng
    {
        private int _state;
        public Rng(int seed) { _state = seed; }

        public double Next()
        {
            unchecked
            {
                _state = _state + 0x6D2B79F5;
                int s = _state;
                int t = (s ^ (int)((uint)s >> 15)) * (1 | s);
                t = (t + ((t ^ (int)((uint)t >> 7)) * (61 | t))) ^ t;
                return (uint)(t ^ (int)((uint)t >> 14)) / 4294967296.0;
            }
        }
    }
}
