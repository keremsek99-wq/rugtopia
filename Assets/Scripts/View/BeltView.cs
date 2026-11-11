using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Belt slots (fixed positions in a row) with the waiting spools; knotted spools show a 'k' badge.
    /// The prototype's looping conveyor animation is week-2 work.</summary>
    public class BeltView : MonoBehaviour
    {
        public float y = -1.0f;
        public float width = 8.4f;

        public void Refresh(GameState g)
        {
            ViewKit.Clear(transform);
            int n = g.BeltSlots;
            float step = Mathf.Min(1.3f, width / Mathf.Max(1, n));
            float x0 = -step * (n - 1) / 2f;
            ViewKit.Quad(transform, "Track", new Vector2(0, y), new Vector2(step * n + 0.55f, 1.55f), ViewKit.Hex("#6A432C"), 0, ArtKit.Wood);
            for (int i = 0; i < n; i++)
            {
                var p = new Vector2(x0 + i * step, y);
                ViewKit.Quad(transform, "Slot" + i, p, new Vector2(step * 0.78f, 1.05f), ViewKit.Hex("#3E2618"), 1, ArtKit.Wood);
                if (i >= g.Belt.Count) continue;
                var s = g.Belt[i];
                float h = step * 0.85f;
                ViewKit.Bobbin(transform, "Spool" + s.Id, p, h, ViewKit.Of(s.Color), 2, 1f);
                ViewKit.Label(transform, "Count" + s.Id, p, s.Left.ToString(), 0.07f, ViewKit.InkFor(s.Color), 6);
                if (s.Knot > 0) ViewKit.Label(transform, "Knot" + s.Id, p + new Vector2(h * 0.4f, h * 0.4f), "k", 0.05f, Color.red, 7);
            }
        }

        public Vector2 SlotLocal(int index, int slots)
        {
            int n = Mathf.Max(1, slots);
            float step = Mathf.Min(1.3f, width / n);
            float x0 = -step * (n - 1) / 2f;
            return new Vector2(x0 + Mathf.Clamp(index, 0, n - 1) * step, y);
        }
    }
}
