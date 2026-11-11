using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Spool lanes: front spool large, the next three smaller; hidden spools show '?'. Converts a world tap to (lane, depth).</summary>
    public class LaneView : MonoBehaviour
    {
        public Rect area = new Rect(-4.3f, -8.6f, 8.6f, 5.4f);
        public const int VisibleDepth = 4;
        int _lanes;

        float LaneW { get { return area.width / Mathf.Max(1, _lanes); } }
        float RowY(int depth) { return area.yMax - 0.8f - depth * 1.15f; }

        public void Refresh(GameState g)
        {
            ViewKit.Clear(transform);
            _lanes = g.Lanes.Count;
            for (int i = 0; i < _lanes; i++)
            {
                float cx = area.xMin + LaneW * (i + 0.5f);
                ViewKit.Quad(transform, "Lane" + i, new Vector2(cx, area.center.y), new Vector2(LaneW * 0.9f, area.height), g.CombArmed ? ViewKit.Hex("#E9D9A6") : ViewKit.Hex("#E6D5BF"), 0);
                var lane = g.Lanes[i];
                for (int d = 0; d < Mathf.Min(VisibleDepth, lane.Count); d++)
                {
                    var s = lane[d];
                    float size = (d == 0 ? 1.0f : 0.75f) * Mathf.Min(1.1f, LaneW * 0.75f);
                    var p = new Vector2(cx, RowY(d));
                    var col = s.Hidden ? ViewKit.Hex("#9C8F80") : ViewKit.Of(s.Color);
                    ViewKit.Quad(transform, "Spool" + s.Id, p, Vector2.one * size, col, 2);
                    ViewKit.Label(transform, "Count", p, s.Hidden ? "?" : s.Left.ToString(), 0.1f, s.Hidden ? Color.white : ViewKit.InkFor(s.Color), 3);
                    if (s.Knot > 0 && !s.Hidden) ViewKit.Label(transform, "Knot", p + new Vector2(size * 0.4f, size * 0.4f), "k", 0.08f, Color.red, 4);
                }
                if (lane.Count > VisibleDepth) ViewKit.Label(transform, "More", new Vector2(cx, area.yMin + 0.3f), "+" + (lane.Count - VisibleDepth), 0.08f, ViewKit.Ink, 3);
            }
        }

        /// <summary>World point -> lane index and depth (depth only matters in comb mode). Returns false outside the lanes.</summary>
        public bool Hit(Vector2 world, out int lane, out int depth)
        {
            lane = -1; depth = 0;
            if (_lanes == 0 || !area.Contains(world)) return false;
            lane = Mathf.Clamp(Mathf.FloorToInt((world.x - area.xMin) / LaneW), 0, _lanes - 1);
            depth = Mathf.Clamp(Mathf.RoundToInt((area.yMax - 0.8f - world.y) / 1.15f), 0, VisibleDepth - 1);
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
