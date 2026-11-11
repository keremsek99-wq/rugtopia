using System.Collections;
using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Draws the rug: woven stitches in colour, unwoven ones faded, the shuttle tip highlighted, covered rows veiled,
    /// plus the NEXT preview strip under the rug. Snaps to state (no animation yet — week 2).</summary>
    public class BoardView : MonoBehaviour
    {
        public Rect area = new Rect(-3.7f, 1.85f, 7.4f, 5.9f);   // world rect for the rug
        public float stripY = 1.05f;

        SpriteRenderer[,] _cells;
        SpriteRenderer _tip;
        SpriteRenderer _ring;
        SpriteRenderer _arrow;
        Transform _strip;
        float _cs; Vector2 _origin;
        int _visualHp = -1;
        int _popRow = -1;
        float _popUntil;

        public void Build(GameState g)
        {
            ViewKit.Clear(transform);
            int rows = g.Grid.Rows, cols = g.Grid.Cols;
            _cs = Mathf.Min(area.width / cols, area.height / rows);
            _origin = new Vector2(area.center.x - _cs * cols / 2f + _cs / 2f, area.center.y + _cs * rows / 2f - _cs / 2f);
            var rug = new Vector2(_cs * cols, _cs * rows);
            var origin = new Vector2(area.center.x, area.center.y);
            ViewKit.Quad(transform, "Frame", origin, rug + new Vector2(0.36f, 0.42f), ViewKit.Hex("#1A0F0B"), 0, ArtKit.UiRound);
            float top = origin.y + rug.y * 0.5f + 0.16f;
            float bot = origin.y - rug.y * 0.5f - 0.16f;
            for (int c = 0; c < cols; c++)
            {
                float x = CellPos(0, c).x;
                ViewKit.Quad(transform, "FringeT" + c, new Vector2(x, top), new Vector2(0.06f, 0.22f), ViewKit.Hex("#D8C3A0"), 1);
                ViewKit.Quad(transform, "FringeB" + c, new Vector2(x, bot), new Vector2(0.06f, 0.22f), ViewKit.Hex("#D8C3A0"), 1);
            }
            ViewKit.Quad(transform, "Backing", origin, rug + new Vector2(0.08f, 0.08f), ViewKit.Hex("#EFE2C8"), 1);
            ViewKit.Quad(transform, "WarpBed", origin, rug, ViewKit.Hex("#6E5A4C"), 1);
            _cells = new SpriteRenderer[rows, cols];
            var cell = Vector2.one * (_cs + 0.02f);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    _cells[r, c] = ViewKit.Quad(transform, "Cell" + r + "_" + c, CellPos(r, c), cell, ViewKit.Unwoven, 2);
            _ring = ViewKit.Quad(transform, "TipRing", Vector2.zero, Vector2.one * _cs * 1.35f, new Color(1f, 0.92f, 0.55f), 4, ViewKit.Ring);
            _tip = ViewKit.Quad(transform, "ShuttleTip", Vector2.zero, Vector2.one * _cs * 0.72f, Color.white, 5);
            _arrow = ViewKit.Quad(transform, "ShuttleArrow", Vector2.zero, new Vector2(_cs * 0.55f, _cs * 0.4f), new Color(1f, 0.95f, 0.7f), 6, ViewKit.Arrow);
            _strip = new GameObject("NextStrip").transform; _strip.SetParent(transform, false);
            Refresh(g);
        }

        public Vector2 CellPos(int r, int c) { return _origin + new Vector2(c * _cs, -r * _cs); }

        public void Refresh(GameState g)
        {
            if (_cells == null) return;
            int rows = g.Grid.Rows, cols = g.Grid.Cols;
            int hp = _visualHp >= 0 ? Mathf.Clamp(_visualHp, 0, g.Grid.Length) : g.Hp;
            int active = g.Grid.RowOf(hp);
            var woven = new bool[rows, cols];
            for (int i = 0; i < hp; i++) woven[g.Grid.PathRow[i], g.Grid.PathCol[i]] = true;
            for (int r = 0; r < rows; r++)
            {
                bool visible = hp >= g.Grid.Length || !g.Grid.CoveredRow[r] || (active >= 0 && r >= active);
                for (int c = 0; c < cols; c++)
                {
                    char col = g.Grid.Color[r, c];
                    var cellSr = _cells[r, c];
                    var tint = ViewKit.Of(col);
                    if (woven[r, c]) cellSr.color = tint;
                    else if (!visible) cellSr.color = ViewKit.Hex("#5A4A40");
                    else { tint.a = 0.38f; cellSr.color = tint; }
                }
            }
            if (hp < g.Grid.Length)
            {
                var pos = CellPos(g.Grid.PathRow[hp], g.Grid.PathCol[hp]);
                int dir = g.Grid.DirectionOfRow(g.Grid.PathRow[hp]);
                var tc = ViewKit.Of(g.Grid.PathColor[hp]);
                _tip.enabled = true;
                _ring.enabled = true;
                _arrow.enabled = true;
                _ring.transform.localPosition = pos;
                _ring.transform.localScale = Vector3.one * _cs * 1.08f;
                _ring.color = new Color(1f, 0.91f, 0.66f, 0.95f);
                _tip.transform.localPosition = pos + new Vector2(-dir * _cs * 0.02f, _cs * 0.58f);
                _tip.transform.localScale = new Vector3(_cs * 0.7f, _cs * 0.22f, 1f);
                _tip.color = Color.Lerp(ViewKit.Hex("#C9A36B"), tc, 0.25f);
                float edge = dir > 0 ? area.xMin - 0.28f : area.xMax + 0.28f;
                _arrow.transform.localPosition = new Vector3(edge, pos.y, -0.2f);
                _arrow.transform.localScale = new Vector3(dir * 0.38f, 0.28f, 1f);
                _arrow.color = ViewKit.Gold;
            }
            else
            {
                _tip.enabled = false;
                _ring.enabled = false;
                _arrow.enabled = false;
            }
            DrawStrip(g);
        }

        void DrawStrip(GameState g)
        {
            ViewKit.Clear(_strip);
            var runs = g.PreviewRuns();
            ViewKit.Quad(_strip, "Bar", new Vector2(area.center.x, stripY), new Vector2(area.width, 0.55f), new Color(0f, 0f, 0f, 0.28f), 3, ArtKit.UiRound);
            ViewKit.Label(_strip, "Next", new Vector2(area.xMin + 0.45f, stripY), "NEXT", 0.045f, new Color(0.96f, 0.91f, 0.81f, 0.75f), 5);
            float x = area.xMin + 1.2f, h = 0.36f;
            foreach (var run in runs)
            {
                if (x > area.xMax - 0.25f) break;
                if (run.Key == '|') { ViewKit.Quad(_strip, "Sep", new Vector2(x + 0.06f, stripY), new Vector2(0.05f, h * 0.7f), ViewKit.Gold, 4); x += 0.24f; continue; }
                if (run.Key == '?') { ViewKit.Label(_strip, "Veiled", new Vector2(x + 0.25f, stripY), "?", 0.1f, ViewKit.Ink, 5); break; }
                float w = Mathf.Clamp(0.38f * run.Value, 0.42f, 1.8f);
                ViewKit.Quad(_strip, "Chip", new Vector2(x + w / 2f, stripY), new Vector2(w, h), ViewKit.Of(run.Key), 4, ArtKit.UiRound);
                ViewKit.Label(_strip, "N", new Vector2(x + w / 2f, stripY), run.Value.ToString(), 0.06f, ViewKit.InkFor(run.Key), 6);
                x += w + 0.16f;
            }
        }

        /// <summary>While a take animates, draw the rug as of this shuttle index. Pass -1 to follow the core.</summary>
        public void SetVisualHp(int hp) { _visualHp = hp; }

        public void PopRow(int row) { _popRow = row; _popUntil = Time.time + 0.32f; }

        public void Shine() { StartCoroutine(ShineCo()); }

        IEnumerator ShineCo()
        {
            var sweep = ViewKit.Quad(transform, "Shine", new Vector2(area.xMin, area.center.y), new Vector2(0.35f, area.height * 0.92f), new Color(1f, 0.97f, 0.85f, 0.55f), 8);
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float k = 1f - (1f - Mathf.Clamp01(t / 0.5f));
                k = 1f - (1f - k) * (1f - k);
                sweep.transform.localPosition = new Vector3(Mathf.Lerp(area.xMin, area.xMax, k), area.center.y, -0.2f);
                yield return null;
            }
            if (sweep != null) Destroy(sweep.gameObject);
        }

        void Update()
        {
            if (_tip != null && _tip.enabled && _ring != null)
            {
                float p = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f);
                var glow = _ring.color;
                glow.a = 0.45f + 0.55f * p;
                _ring.color = glow;
                _ring.transform.localScale = Vector3.one * _cs * (1.28f + 0.18f * p);
            }
            if (_cells == null || Time.time > _popUntil || _popRow < 0) return;
            float s = 1f + 0.22f * Mathf.Sin((1f - (_popUntil - Time.time) / 0.32f) * Mathf.PI);
            int cols = _cells.GetLength(1);
            for (int c = 0; c < cols; c++)
                if (_cells[_popRow, c] != null) _cells[_popRow, c].transform.localScale = Vector3.one * _cs * 0.94f * s;
        }
    }
}
