using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>A flat rounded belt, the same capsule the prototype draws.</summary>
    public class BeltView : MonoBehaviour
    {
        public float y = -1.55f;
        public float width = 7.2f;

        public void Refresh(GameState g)
        {
            ViewKit.Clear(transform);
            int n = g.BeltSlots;
            float step = Mathf.Min(1.15f, (width - 1.2f) / Mathf.Max(1, n));
            float x0 = -step * (n - 1) / 2f;
            ViewKit.Quad(transform, "Track", new Vector2(0, y), new Vector2(step * n + 2.4f, 2.15f), ViewKit.Hex("#3D2A20"), 0, ArtKit.UiRound);
            ViewKit.Quad(transform, "Inner", new Vector2(0, y), new Vector2(step * n + 1.7f, 1.35f), ViewKit.Hex("#2A1C16"), 1, ArtKit.UiRound);
            ViewKit.Label(transform, "Cap", new Vector2(0, y + 0.95f), "BELT  " + g.Belt.Count + "/" + n, 0.05f, ViewKit.Cream, 3);
            for (int i = 0; i < n; i++)
            {
                var p = new Vector2(x0 + i * step, y);
                ViewKit.Quad(transform, "Slot" + i, p, Vector2.one * 0.72f, new Color(1f, 0.9f, 0.78f, 0.18f), 2, ArtKit.Card);
                if (i >= g.Belt.Count) continue;
                var s = g.Belt[i];
                float h = 0.7f;
                ViewKit.Bobbin(transform, "Spool" + s.Id, p, h, ViewKit.Of(s.Color), 4, 1f);
                ViewKit.Label(transform, "Count" + s.Id, p + new Vector2(0.22f, -0.22f), s.Left.ToString(), 0.04f, Color.white, 6);
            }
        }

        public Vector2 SlotLocal(int index, int slots)
        {
            int n = Mathf.Max(1, slots);
            float step = Mathf.Min(1.15f, (width - 1.2f) / n);
            float x0 = -step * (n - 1) / 2f;
            return new Vector2(x0 + Mathf.Clamp(index, 0, n - 1) * step, y);
        }
    }
}
