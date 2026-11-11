using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Dark is the current warm brown. Light is a soft cream/sand for comparison.</summary>
    public static class ThemeConfig
    {
        public enum Theme { Dark, Light }

        public static Theme Current = Theme.Dark;
        public static bool IsLight { get { return Current == Theme.Light; } }

        public static Color BgTop { get { return IsLight ? ViewKit.Hex("#F6E7CF") : ViewKit.Hex("#4A2C20"); } }
        public static Color BgBot { get { return IsLight ? ViewKit.Hex("#E7D3B0") : ViewKit.Hex("#22140F"); } }
        public static Color Ink { get { return IsLight ? ViewKit.Hex("#2B1A14") : ViewKit.Cream; } }
        public static Color Muted { get { return IsLight ? ViewKit.Hex("#6B4A35") : ViewKit.Gold; } }
        public static Color Lane { get { return IsLight ? new Color(1f, 0.97f, 0.93f, 0.94f) : new Color(0.08f, 0.04f, 0.03f, 0.72f); } }
        public static Color Belt { get { return IsLight ? ViewKit.Hex("#C4A27A") : ViewKit.Hex("#3D2A20"); } }
        public static Color BeltHole { get { return IsLight ? ViewKit.Hex("#F3E6CF") : ViewKit.Hex("#241610"); } }
        public static Color Button { get { return IsLight ? ViewKit.Hex("#E7D3B0") : ViewKit.Hex("#3D2A20"); } }
        public static Color ButtonText { get { return IsLight ? ViewKit.Hex("#2B1A14") : ViewKit.Cream; } }
        public static Color Bubble { get { return IsLight ? ViewKit.Hex("#FFF8EE") : new Color(0.08f, 0.04f, 0.02f, 0.92f); } }
        public static Color BubbleText { get { return IsLight ? ViewKit.Hex("#2B1A14") : Color.white; } }
        public static Color BubbleEdge { get { return IsLight ? ViewKit.Hex("#8A5A32") : ViewKit.Gold; } }
    }
}
