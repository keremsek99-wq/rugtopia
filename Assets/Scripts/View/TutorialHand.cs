using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Points at a lane only when that lane's front spool matches the glowing stitch.</summary>
    public class TutorialHand : MonoBehaviour
    {
        Vector2 _at;
        bool _on;

        /// <summary>Lane index, or -1 when no front spool is the correct colour.</summary>
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
                ViewKit.Quad(transform, "Pointer", Vector2.zero, new Vector2(0.72f, 1.05f), Color.white, 40, ArtKit.Hand);
            gameObject.SetActive(true);
            transform.localPosition = _at + new Vector2(0f, 0.62f);
        }

        public void Hide()
        {
            _on = false;
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (!_on) return;
            float b = 0.5f + 0.5f * Mathf.Sin(Time.time * 5f);
            transform.localPosition = _at + new Vector2(0f, 0.55f + b * 0.16f);
        }
    }
}
