using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Wide capsule conveyor. Slot rings sit on the brown track, not in the hole.</summary>
    public class BeltView : MonoBehaviour
    {
        public float y = -1.15f;
        public float width = 8.6f;
        const float Height = 2.45f;

        public void Refresh(GameState g)
        {
            ViewKit.Clear(transform);
            int n = Mathf.Max(1, g.BeltSlots);
            ViewKit.Quad(transform, "Track", new Vector2(0, y), new Vector2(width, Height), ViewKit.Hex("#3D2A20"), 0, ArtKit.UiRound);
            ViewKit.Quad(transform, "Inner", new Vector2(0, y), new Vector2(width - 0.95f, Height - 0.85f), ViewKit.Hex("#241610"), 1, ArtKit.UiRound);
            ViewKit.Label(transform, "Cap", new Vector2(0, y + 0.28f), "BELT", 0.09f, new Color(1f, 0.9f, 0.78f, 0.7f), 3);
            ViewKit.Label(transform, "Count", new Vector2(0, y - 0.2f), g.Belt.Count + " / " + n, 0.16f, ViewKit.Cream, 3);
            for (int i = 0; i < n; i++)
            {
                var p = OnTrack(i / (float)n);
                ViewKit.Quad(transform, "Slot" + i, p, Vector2.one * 0.78f, new Color(1f, 0.9f, 0.78f, 0.7f), 2, Glyphs.Ring);
                if (i >= g.Belt.Count) continue;
                var s = g.Belt[i];
                ViewKit.SpoolIcon(transform, "Spool" + s.Id, p, 0.9f, s.Color, s.Left.ToString(), false, 4, 1f);
            }
        }

        public Vector2 SlotLocal(int index, int slots)
        {
            int n = Mathf.Max(1, slots);
            return OnTrack(Mathf.Clamp(index, 0, n - 1) / (float)n);
        }

        /// <summary>Point on the center of the brown band. u = 0 is the left end of the top edge, going clockwise.</summary>
        Vector2 OnTrack(float u)
        {
            float rad = (Height - 0.42f) * 0.5f;
            float straight = Mathf.Max(0.4f, width - Height);
            float per = straight * 2f + Mathf.PI * 2f * rad;
            float d = Mathf.Repeat(u, 1f) * per;
            if (d < straight) return new Vector2(-straight * 0.5f + d, y + rad);
            d -= straight;
            if (d < Mathf.PI * rad)
            {
                float a = Mathf.PI * 0.5f - d / rad;
                return new Vector2(straight * 0.5f + Mathf.Cos(a) * rad, y + Mathf.Sin(a) * rad);
            }
            d -= Mathf.PI * rad;
            if (d < straight) return new Vector2(straight * 0.5f - d, y - rad);
            d -= straight;
            float b = -Mathf.PI * 0.5f - d / rad;
            return new Vector2(-straight * 0.5f + Mathf.Cos(b) * rad, y + Mathf.Sin(b) * rad);
        }
    }
}
