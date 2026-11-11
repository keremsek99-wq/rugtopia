namespace Rugtopia.Core
{
    public sealed class Spool
    {
        public int Id;        // stable id (= deal sequence index) for views
        public char Color;
        public int Left;      // threads left
        public int Total;     // threads at deal time (badge)
        public bool Hidden;   // shows '?' until it reaches the lane front
        public int Knot;      // >0: on the belt it fires only after this many more rows are finished
        public int Seq;       // order in the generated sequence (used by the Shuttle booster)
        public int Order;     // take order on the belt (oldest same-colour spool fires first)

        public Spool Clone() { return (Spool)MemberwiseClone(); }
        public override string ToString() { return Color.ToString() + Left + (Knot > 0 ? "k" + Knot : ""); }
    }
}
