using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Large rounded lane cards. The hinted lane gets a gold edge and a big front spool.</summary>
    public class LaneView : MonoBehaviour
    {
        public Rect area = new Rect(-4.3f, -9.35f, 8.6f, 4.03f);
        int _lanes;
        int _visible = 3;
        float _front = 1.6f;
        float _back = 0.7f;
        Transform _pulse;
        Vector3 _pulseBase;

        float Pitch { get { return area.width / Mathf.Max(1, _lanes); } }

        /// <summary>Cards stay under the belt. Boosters, when shown, own the bottom strip.</summary>
        public void Fit(int lanes, bool boosters)
        {
            _lanes = Mathf.Max(1, lanes);
            // Belt holders sit higher now. Boosters own a 44pt row at the bottom.
            float yMax = -5.32f;
            float yMin = boosters ? -7.84f : -9.35f;
            area = new Rect(-4.3f, yMin, 8.6f, yMax - yMin);
        }

        float RowY(int depth)
        {
            if (depth <= 0) return area.yMax - 0.08f - _front * 0.5f;
            return area.yMax - 0.08f - _front - 0.04f - _back * 0.5f - (depth - 1) * (_back + 0.04f);
        }

        float SpoolH(int depth) { return depth == 0 ? _front : _back; }

        static bool HasExtra(GameState g, int shown)
        {
            for (int i = 0; i < g.Lanes.Count; i++) if (g.Lanes[i].Count > shown) return true;
            return false;
        }

        public void Refresh(GameState g)
        {
            ViewKit.Clear(transform);
            _pulse = null;
            _lanes = Mathf.Max(1, g.Lanes.Count);
            int hint = TutorialHand.LaneFor(g);
            float gap = _lanes >= 5 ? 0.1f : _lanes >= 4 ? 0.14f : 0.2f;
            float cardW = Pitch - gap;
            int maxBack = 2;
            int deepest = 1;
            for (int i = 0; i < g.Lanes.Count; i++) deepest = Mathf.Max(deepest, Mathf.Min(maxBack + 1, g.Lanes[i].Count));
            bool extra = HasExtra(g, deepest);
            float label = extra ? 0.2f : 0.06f;
            const float ratio = 0.3f;
            // Front spool width is height * 0.78. 1.34 tall is about 44pt on a 9:19.5 phone.
            float minFront = _lanes >= 5 ? 1.34f : _lanes >= 4 ? 1.5f : 1.7f;
            int backs = Mathf.Max(0, deepest - 1);
            while (backs > 0 && 0.08f + minFront + backs * (minFront * ratio + 0.04f) + label > area.height) backs--;
            float usable = area.height - 0.08f - label - backs * 0.04f;
            float maxFront = usable / (1f + backs * ratio);
            float badgeCap = (cardW * 0.5f) / 0.56f;
            float desired = _lanes >= 5 ? 1.45f : _lanes >= 4 ? 1.6f : _lanes >= 3 ? 1.75f : 1.95f;
            _front = Mathf.Min(desired, Mathf.Max(0.01f, maxFront), badgeCap);
            _back = _front * ratio;
            _visible = backs + 1;
            float edge = Mathf.Min(0.05f, gap * 0.35f);
            for (int i = 0; i < _lanes; i++)
            {
                float cx = area.xMin + Pitch * (i + 0.5f);
                var lane = g.Lanes[i];
                bool hot = i == hint;
                var shadow = new Color(0.28f, 0.14f, 0.06f, ThemeConfig.IsLight ? 0.28f : 0.2f);
                ViewKit.Quad(transform, "Shade" + i, new Vector2(cx, area.center.y - 0.04f), new Vector2(cardW + edge, area.height), shadow, -1, ArtKit.Shadow);
                ViewKit.Quad(transform, "Edge" + i, new Vector2(cx, area.center.y), new Vector2(cardW + edge, area.height - 0.04f), ThemeConfig.LaneEdge, 0, ArtKit.UiRound);
                if (hot) ViewKit.Quad(transform, "Gold" + i, new Vector2(cx, area.center.y), new Vector2(cardW + edge + 0.04f, area.height), ViewKit.Gold, 0, ArtKit.UiRound);
                ViewKit.Quad(transform, "Lane" + i, new Vector2(cx, area.center.y), new Vector2(cardW, area.height - 0.1f), ThemeConfig.Lane, 1, ArtKit.UiRound);
                int shown = Mathf.Min(_visible, lane.Count);
                for (int d = shown - 1; d >= 0; d--)
                {
                    var s = lane[d];
                    float size = SpoolH(d);
                    var p = new Vector2(cx, RowY(d));
                    ViewKit.SpoolIcon(transform, "Spool" + s.Id, p, size, s.Color, s.Hidden ? "?" : s.Left.ToString(), s.Hidden, 2, d == 0 ? 1f : 0.8f);
                    if (d == 0 && g.TipColor != null && !s.Hidden && s.Color == g.TipColor.Value)
                    {
                        var glow = ViewKit.Quad(transform, "Glow" + i, p, new Vector2(size * 0.9f, size * 0.95f), ViewKit.Gold, 1, ArtKit.UiRound);
                        _pulse = glow.transform;
                        _pulseBase = _pulse.localScale;
                    }
                }
                if (lane.Count > shown) ViewKit.Label(transform, "More", new Vector2(cx, area.yMin + 0.16f), "+" + (lane.Count - shown), 0.09f, ThemeConfig.Ink, 3);
                if (lane.Count == 0) ViewKit.Label(transform, "Empty" + i, new Vector2(cx, RowY(0)), "empty", _lanes >= 4 ? 0.05f : 0.08f, ThemeConfig.Muted, 3);
            }
        }

        public bool Hit(Vector2 world, out int lane, out int depth)
        {
            lane = -1; depth = 0;
            if (_lanes == 0 || !area.Contains(world)) return false;
            lane = Mathf.Clamp(Mathf.FloorToInt((world.x - area.xMin) / Pitch), 0, _lanes - 1);
            depth = 0;
            float best = float.MaxValue;
            for (int d = 0; d < _visible; d++)
            {
                float dist = Mathf.Abs(world.y - RowY(d));
                if (dist < best) { best = dist; depth = d; }
            }
            return true;
        }

        void Update()
        {
            if (_pulse == null) return;
            float k = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f);
            _pulse.localScale = _pulseBase * (1f + 0.1f * k);
            var sr = _pulse.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var c = ViewKit.Gold;
                c.a = 0.25f + 0.6f * k;
                sr.color = c;
            }
        }

        public Vector2 FrontLocal(int lane)
        {
            if (_lanes <= 0) return new Vector2(area.center.x, area.center.y);
            float cx = area.xMin + Pitch * (Mathf.Clamp(lane, 0, _lanes - 1) + 0.5f);
            return new Vector2(cx, RowY(0));
        }
    }
}
