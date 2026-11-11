using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Dark lane columns, matching the prototype: a large front spool and up to three behind it.</summary>
    public class LaneView : MonoBehaviour
    {
        public Rect area = new Rect(-4.05f, -9.55f, 8.1f, 3.35f);
        public const int VisibleDepth = 4;
        const float Step = 0.62f;
        int _lanes;

        float LaneW { get { return area.width / Mathf.Max(1, _lanes); } }
        float RowY(int depth) { return area.yMax - 0.55f - depth * Step; }

        public void Refresh(GameState g)
        {
            ViewKit.Clear(transform);
            _lanes = g.Lanes.Count;
            for (int i = 0; i < _lanes; i++)
            {
                float cx = area.xMin + LaneW * (i + 0.5f);
                var lane = g.Lanes[i];
                var back = new Color(0f, 0f, 0f, 0.28f);
                ViewKit.Quad(transform, "Lane" + i, new Vector2(cx, area.center.y), new Vector2(LaneW * 0.9f, area.height), back, 0, ArtKit.UiRound);
                int shown = Mathf.Min(VisibleDepth, lane.Count);
                for (int d = shown - 1; d >= 0; d--)
                {
                    var s = lane[d];
                    float size = d == 0 ? 0.95f : 0.62f - d * 0.04f;
                    var p = new Vector2(cx, RowY(d));
                    var col = s.Hidden ? ViewKit.Hex("#7A6A60") : ViewKit.Of(s.Color);
                    float bright = d == 0 ? 1f : 0.72f;
                    ViewKit.Bobbin(transform, "Spool" + s.Id, p, size, col, 2, bright);
                    if (d != 0) continue;
                    ViewKit.Label(transform, "Count" + s.Id, p + new Vector2(size * 0.28f, -size * 0.28f), s.Hidden ? "?" : s.Left.ToString(), 0.045f, Color.white, 6);
                    if (s.Knot > 0 && !s.Hidden) ViewKit.Label(transform, "Knot" + s.Id, p + new Vector2(-size * 0.3f, size * 0.3f), "k", 0.04f, ViewKit.Gold, 7);
                }
                if (lane.Count > VisibleDepth) ViewKit.Label(transform, "More", new Vector2(cx, area.yMin + 0.18f), "+" + (lane.Count - VisibleDepth), 0.045f, ViewKit.Cream, 3);
                if (lane.Count == 0) ViewKit.Label(transform, "Empty" + i, new Vector2(cx, RowY(0)), "empty", 0.04f, new Color(1f, 0.9f, 0.78f, 0.35f), 3);
            }
        }

        public bool Hit(Vector2 world, out int lane, out int depth)
        {
            lane = -1; depth = 0;
            if (_lanes == 0 || !area.Contains(world)) return false;
            lane = Mathf.Clamp(Mathf.FloorToInt((world.x - area.xMin) / LaneW), 0, _lanes - 1);
            depth = Mathf.Clamp(Mathf.RoundToInt((area.yMax - 0.55f - world.y) / Step), 0, VisibleDepth - 1);
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
