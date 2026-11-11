using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Points at a lane only when that lane's front spool matches the glowing stitch.</summary>
    public class TutorialHand : MonoBehaviour
    {
        Vector2 _at;
        bool _on;

        public static int LaneFor(GameState g)
        {
            if (g == null || g.Status != GameStatus.Playing) return -1;
            char? tip = g.TipColor;
            if (tip == null) return -1;
            for (int i = 0; i < g.Lanes.Count; i++)
            {
                if (g.Lanes[i].Count == 0 || !g.CanTake(i, 0)) continue;
                var front = g.Lanes[i][0];
                if (!front.Hidden && front.Color == tip.Value) return i;
            }
            return -1;
        }

        public void Show(Vector2 local)
        {
            _at = local;
            _on = true;
            if (transform.childCount == 0)
            {
                ViewKit.Quad(transform, "PalmBack", new Vector2(0.12f, 0.22f), new Vector2(0.42f, 0.36f), ViewKit.Hex("#5B3A29"), 40, ArtKit.UiRound);
                ViewKit.Quad(transform, "Palm", new Vector2(0.12f, 0.24f), new Vector2(0.34f, 0.28f), ViewKit.Hex("#FFE2C4"), 41, ArtKit.UiRound);
                ViewKit.Quad(transform, "FingerBack", new Vector2(0f, 0.42f), new Vector2(0.2f, 0.48f), ViewKit.Hex("#5B3A29"), 40, ArtKit.UiRound);
                ViewKit.Quad(transform, "Finger", new Vector2(0f, 0.44f), new Vector2(0.14f, 0.4f), ViewKit.Hex("#FFE2C4"), 41, ArtKit.UiRound);
            }
            gameObject.SetActive(true);
            transform.localPosition = _at + new Vector2(0.15f, 0.15f);
        }

        public void Hide()
        {
            _on = false;
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (!_on) return;
            float b = Mathf.Sin(Time.time * 6f) * 0.06f;
            transform.localPosition = _at + new Vector2(0.15f, 0.2f + b);
        }
    }
}
