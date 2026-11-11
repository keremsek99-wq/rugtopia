using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Compact spool lanes. The card hugs the visible stack instead of a tall empty panel.</summary>
    public class LaneView : MonoBehaviour
    {
        public Rect area = new Rect(-4.2f, -8.35f, 8.4f, 2.85f);
        public const int VisibleDepth = 4;
        const float Step = 0.62f;
        int _lanes;

        float LaneW { get { return area.width / Mathf.Max(1, _lanes); } }
        float RowY(int depth) { return area.yMax - 0.42f - depth * Step; }

        public void Refresh(GameState g)
        {
            ViewKit.Clear(transform);
            _lanes = g.Lanes.Count;
            for (int i = 0; i < _lanes; i++)
            {
                float cx = area.xMin + LaneW * (i + 0.5f);
                var lane = g.Lanes[i];
                int shown = Mathf.Min(VisibleDepth, lane.Count);
                if (shown > 0)
                {
                    float top = RowY(0) + 0.48f;
                    float bot = RowY(shown - 1) - 0.42f;
                    var card = ViewKit.Hex("#FFF6EA");
                    if (g.CombArmed) card = ViewKit.Hex("#F5E2A8");
                    ViewKit.Quad(transform, "Lane" + i, new Vector2(cx, (top + bot) * 0.5f), new Vector2(Mathf.Min(LaneW * 0.86f, 1.7f), top - bot), card, 0, ArtKit.Card);
                }
                for (int d = 0; d < shown; d++)
                {
                    var s = lane[d];
                    float size = d == 0 ? 0.92f : 0.68f;
                    var p = new Vector2(cx, RowY(d));
                    var col = s.Hidden ? ViewKit.Hex("#9C8F80") : ViewKit.Of(s.Color);
                    float bright = d == 0 ? 1f : 0.78f;
                    ViewKit.Bobbin(transform, "Spool" + s.Id, p, size, col, 2, bright);
                    if (d != 0) continue;
                    ViewKit.Label(transform, "Count" + s.Id, p, s.Hidden ? "?" : s.Left.ToString(), 0.055f, s.Hidden ? Color.white : ViewKit.InkFor(s.Color), 6);
                    if (s.Knot > 0 && !s.Hidden) ViewKit.Label(transform, "Knot" + s.Id, p + new Vector2(size * 0.42f, size * 0.42f), "k", 0.045f, Color.red, 7);
                }
                if (lane.Count > VisibleDepth) ViewKit.Label(transform, "More", new Vector2(cx, area.yMin + 0.16f), "+" + (lane.Count - VisibleDepth), 0.05f, ViewKit.Ink, 3);
            }
        }

        public bool Hit(Vector2 world, out int lane, out int depth)
        {
            lane = -1; depth = 0;
            if (_lanes == 0 || !area.Contains(world)) return false;
            lane = Mathf.Clamp(Mathf.FloorToInt((world.x - area.xMin) / LaneW), 0, _lanes - 1);
            depth = Mathf.Clamp(Mathf.RoundToInt((area.yMax - 0.42f - world.y) / Step), 0, VisibleDepth - 1);
            return true;
        }

        public Vector2 FrontLocal(int lane)
        {
            if (_lanes <= 0) return new Vector2(area.center.x, area.center.y);
            float cx = area.xMin + LaneW * (Mathf.Clamp(lane, 0, _lanes - 1) + 0.5f);
            return new Vector2(cx, RowY(0));
        }
    }
}
