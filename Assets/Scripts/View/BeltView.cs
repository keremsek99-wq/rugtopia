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
            ViewKit.Quad(transform, "Track", new Vector2(0, y), new Vector2(step * n + 0.3f, 1.3f), ViewKit.Hex("#6B4A35"), 0);
            for (int i = 0; i < n; i++)
            {
                var p = new Vector2(x0 + i * step, y);
                ViewKit.Quad(transform, "Slot" + i, p, Vector2.one * step * 0.8f, ViewKit.Hex("#8C6A52"), 1);
                if (i >= g.Belt.Count) continue;
                var s = g.Belt[i];
                ViewKit.Quad(transform, "Spool" + s.Id, p, Vector2.one * step * 0.7f, ViewKit.Of(s.Color), 2);
                ViewKit.Label(transform, "Count", p, s.Left.ToString(), 0.1f, ViewKit.InkFor(s.Color), 3);
                if (s.Knot > 0) ViewKit.Label(transform, "Knot", p + new Vector2(step * 0.3f, step * 0.3f), "k", 0.08f, Color.red, 4);
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
