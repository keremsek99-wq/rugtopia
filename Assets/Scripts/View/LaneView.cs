using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Large rounded lane cards. The hinted lane gets a gold edge and a big front spool.</summary>
    public class LaneView : MonoBehaviour
    {
        public Rect area = new Rect(-4.15f, -9.95f, 8.3f, 4.35f);
        public const int VisibleDepth = 4;
        int _lanes;

        float LaneW { get { return area.width / Mathf.Max(1, _lanes); } }

        float RowY(int depth)
        {
            float top = area.yMax - 0.12f;
            if (depth <= 0) return top - 1.15f;
            return top - 2.55f - (depth - 1) * 0.85f;
        }

        static float SpoolH(int depth) { return depth == 0 ? 2.2f : 1.05f; }

        public void Refresh(GameState g)
        {
            ViewKit.Clear(transform);
            _lanes = g.Lanes.Count;
            int hint = TutorialHand.LaneFor(g);
            for (int i = 0; i < _lanes; i++)
            {
                float cx = area.xMin + LaneW * (i + 0.5f);
                float cardW = LaneW * 0.9f;
                var lane = g.Lanes[i];
                bool hot = i == hint;
                if (hot) ViewKit.Quad(transform, "Gold" + i, new Vector2(cx, area.center.y), new Vector2(cardW + 0.12f, area.height + 0.1f), ViewKit.Gold, 0, ArtKit.UiRound);
                ViewKit.Quad(transform, "Lane" + i, new Vector2(cx, area.center.y), new Vector2(cardW, area.height), new Color(0.08f, 0.04f, 0.03f, 0.72f), 1, ArtKit.UiRound);
                int shown = Mathf.Min(VisibleDepth, lane.Count);
                for (int d = shown - 1; d >= 0; d--)
                {
                    var s = lane[d];
                    float size = SpoolH(d);
                    var p = new Vector2(cx, RowY(d));
                    ViewKit.SpoolIcon(transform, "Spool" + s.Id, p, size, s.Color, s.Hidden ? "?" : s.Left.ToString(), s.Hidden, 2, d == 0 ? 1f : 0.8f);
                }
                if (lane.Count > VisibleDepth) ViewKit.Label(transform, "More", new Vector2(cx, area.yMin + 0.22f), "+" + (lane.Count - VisibleDepth), 0.1f, ViewKit.Cream, 3);
                if (lane.Count == 0) ViewKit.Label(transform, "Empty" + i, new Vector2(cx, RowY(0)), "empty", 0.1f, new Color(1f, 0.9f, 0.78f, 0.35f), 3);
            }
        }

        public bool Hit(Vector2 world, out int lane, out int depth)
        {
            lane = -1; depth = 0;
            if (_lanes == 0 || !area.Contains(world)) return false;
            lane = Mathf.Clamp(Mathf.FloorToInt((world.x - area.xMin) / LaneW), 0, _lanes - 1);
            depth = 0;
            float best = float.MaxValue;
            for (int d = 0; d < VisibleDepth; d++)
            {
                float dist = Mathf.Abs(world.y - RowY(d));
                if (dist < best) { best = dist; depth = d; }
            }
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
