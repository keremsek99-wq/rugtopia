using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Compact row of large slots sitting just above the lanes.</summary>
    public class BeltView : MonoBehaviour
    {
        public float y = -4.85f;
        public float width = 8.4f;

        public void Refresh(GameState g)
        {
            ViewKit.Clear(transform);
            int n = Mathf.Max(1, g.BeltSlots);
            float slot = Mathf.Min(2.15f, (width - 0.2f * (n - 1)) / n);
            float gap = 0.22f;
            float total = n * slot + (n - 1) * gap;
            float x0 = -total * 0.5f + slot * 0.5f;
            ViewKit.Quad(transform, "Track", new Vector2(0f, y), new Vector2(width, slot + 0.35f), ThemeConfig.Belt, 0, ArtKit.UiRound);
            ViewKit.Label(transform, "Cap", new Vector2(0f, y + slot * 0.72f), "BELT  " + g.Belt.Count + "/" + n, 0.1f, ThemeConfig.Ink, 3);
            for (int i = 0; i < n; i++)
            {
                var p = new Vector2(x0 + i * (slot + gap), y);
                ViewKit.Quad(transform, "Slot" + i, p, new Vector2(slot * 0.92f, slot * 0.92f), ThemeConfig.BeltHole, 1, ArtKit.UiRound);
                ViewKit.Quad(transform, "Ring" + i, p, new Vector2(slot * 0.78f, slot * 0.78f), new Color(1f, 0.9f, 0.78f, ThemeConfig.IsLight ? 0.45f : 0.35f), 2, Glyphs.Ring);
                if (i >= g.Belt.Count) continue;
                var s = g.Belt[i];
                ViewKit.SpoolIcon(transform, "Spool" + s.Id, p, slot * 0.7f, s.Color, s.Left.ToString(), false, 4, 1f);
            }
        }

        public Vector2 SlotLocal(int index, int slots)
        {
            int n = Mathf.Max(1, slots);
            float slot = Mathf.Min(2.15f, (width - 0.2f * (n - 1)) / n);
            float gap = 0.22f;
            float total = n * slot + (n - 1) * gap;
            float x0 = -total * 0.5f + slot * 0.5f;
            return new Vector2(x0 + Mathf.Clamp(index, 0, n - 1) * (slot + gap), y);
        }
    }
}
