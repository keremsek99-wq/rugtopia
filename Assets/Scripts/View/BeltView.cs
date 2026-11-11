using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Oval conveyor: a capsule track, dashed slot rings, and a large BELT count.</summary>
    public class BeltView : MonoBehaviour
    {
        public float y = -1.35f;
        public float width = 7.4f;

        public void Refresh(GameState g)
        {
            ViewKit.Clear(transform);
            int n = Mathf.Max(1, g.BeltSlots);
            float rx = width * 0.46f, ry = 1.05f;
            ViewKit.Quad(transform, "Track", new Vector2(0, y), new Vector2(rx * 2f, ry * 2f), ViewKit.Hex("#3D2A20"), 0, ArtKit.UiRound);
            ViewKit.Quad(transform, "Inner", new Vector2(0, y), new Vector2(rx * 2f - 0.7f, ry * 2f - 0.55f), ViewKit.Hex("#241610"), 1, ArtKit.UiRound);
            ViewKit.Label(transform, "Cap", new Vector2(0, y + 0.28f), "BELT", 0.09f, new Color(1f, 0.9f, 0.78f, 0.7f), 3);
            ViewKit.Label(transform, "Count", new Vector2(0, y - 0.22f), g.Belt.Count + " / " + n, 0.16f, ViewKit.Cream, 3);
            for (int i = 0; i < n; i++)
            {
                float ang = -Mathf.PI * 0.5f + (i + 0.5f) / n * Mathf.PI * 2f;
                var p = new Vector2(Mathf.Cos(ang) * (rx - 0.15f), y + Mathf.Sin(ang) * (ry - 0.12f));
                ViewKit.Quad(transform, "Slot" + i, p, Vector2.one * 0.85f, new Color(1f, 0.9f, 0.78f, 0.45f), 2, Glyphs.Ring);
                if (i >= g.Belt.Count) continue;
                var s = g.Belt[i];
                ViewKit.SpoolIcon(transform, "Spool" + s.Id, p, 0.85f, s.Color, s.Left.ToString(), false, 4, 1f);
            }
        }

        public Vector2 SlotLocal(int index, int slots)
        {
            int n = Mathf.Max(1, slots);
            float rx = width * 0.46f, ry = 1.05f;
            float ang = -Mathf.PI * 0.5f + (Mathf.Clamp(index, 0, n - 1) + 0.5f) / n * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(ang) * (rx - 0.15f), y + Mathf.Sin(ang) * (ry - 0.12f));
        }
    }
}
