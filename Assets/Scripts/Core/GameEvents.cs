namespace Rugtopia.Core
{
    public enum GameStatus { Playing, Won, Failed }
    public enum EventKind { SpoolTaken, StitchWoven, RowCompleted, KnotLoosened, SpoolEmptied, SpoolRevealed, SlotAdded, ShuttleBooster, Won, Failed, Continued }

    /// <summary>What happened during the last action, in order, so a view can animate it.</summary>
    public struct GameEvent
    {
        public EventKind Kind;
        public int SpoolId;   // -1 if none
        public int Lane;      // SpoolTaken / SpoolRevealed
        public int Row, Col;  // StitchWoven / RowCompleted (Row)
        public int PathIndex; // StitchWoven
        public override string ToString() { return Kind + "(spool " + SpoolId + ", lane " + Lane + ", r" + Row + " c" + Col + ")"; }
    }
}
