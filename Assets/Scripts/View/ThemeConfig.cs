using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Light is the default cream/sand. Dark stays available from the menu.</summary>
    public static class ThemeConfig
    {
        public enum Theme { Dark, Light }

        public static Theme Current = Theme.Light;
        public static bool IsLight { get { return Current == Theme.Light; } }

        public static void Load(string saved)
        {
            Current = saved == "dark" ? Theme.Dark : Theme.Light;
        }

        public static string Saved { get { return IsLight ? "light" : "dark"; } }

        public static Color BgTop { get { return IsLight ? ViewKit.Hex("#F6E7CF") : ViewKit.Hex("#4A2C20"); } }
        public static Color BgBot { get { return IsLight ? ViewKit.Hex("#E7D3B0") : ViewKit.Hex("#22140F"); } }
        public static Color Ink { get { return IsLight ? ViewKit.Hex("#2B1A14") : ViewKit.Cream; } }
        public static Color Muted { get { return IsLight ? ViewKit.Hex("#5C3B28") : ViewKit.Gold; } }
        public static Color Lane { get { return IsLight ? ViewKit.Hex("#D2AE84") : new Color(0.08f, 0.04f, 0.03f, 0.72f); } }
        public static Color LaneEdge { get { return IsLight ? ViewKit.Hex("#6B4330") : new Color(0f, 0f, 0f, 0.35f); } }
        public static Color RugFrame { get { return IsLight ? ViewKit.Hex("#6B4330") : ViewKit.Hex("#1A0F0B"); } }
        public static Color Card { get { return IsLight ? ViewKit.Hex("#FFF8EE") : ViewKit.Hex("#F3E6CF"); } }
        public static Color Belt { get { return IsLight ? ViewKit.Hex("#C4A27A") : ViewKit.Hex("#3D2A20"); } }
        public static Color BeltHole { get { return IsLight ? ViewKit.Hex("#E8D2B4") : ViewKit.Hex("#241610"); } }
        public static Color Button { get { return IsLight ? ViewKit.Hex("#6B4330") : ViewKit.Hex("#3D2A20"); } }
        public static Color ButtonText { get { return ViewKit.Cream; } }
        public static Color Bubble { get { return IsLight ? ViewKit.Hex("#FFF8EE") : new Color(0.08f, 0.04f, 0.02f, 0.92f); } }
        public static Color BubbleText { get { return IsLight ? ViewKit.Hex("#2B1A14") : Color.white; } }
        public static Color BubbleEdge { get { return IsLight ? ViewKit.Hex("#6B4330") : ViewKit.Gold; } }
    }
}
