using Rugtopia.Core;
using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Bouncing hand that points at the lane whose front spool matches the shuttle tip.</summary>
    public class TutorialHand : MonoBehaviour
    {
        Vector2 _at;
        bool _on;

        public void Show(Vector2 local)
        {
            _at = local;
            _on = true;
            if (transform.childCount == 0)
            {
                ViewKit.Quad(transform, "Palm", Vector2.zero, new Vector2(0.55f, 0.62f), ViewKit.Hex("#F6ECD9"), 40);
                ViewKit.Quad(transform, "Finger", new Vector2(0.05f, 0.48f), new Vector2(0.22f, 0.55f), ViewKit.Hex("#E7C8A4"), 41);
            }
            gameObject.SetActive(true);
            transform.localPosition = _at + Vector2.up * 0.15f;
        }

        public static int LaneFor(GameState g)
        {
            char? tip = g.TipColor;
            if (tip == null) return 0;
            for (int i = 0; i < g.Lanes.Count; i++)
            {
                if (g.Lanes[i].Count == 0) continue;
                var front = g.Lanes[i][0];
                if (!front.Hidden && front.Color == tip.Value) return i;
            }
            return 0;
        }

        public void Hide()
        {
            _on = false;
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (!_on) return;
            float b = Mathf.Sin(Time.time * 5f);
            float e = b * b;
            transform.localPosition = _at + new Vector2(0.15f, 0.55f + e * 0.28f);
        }
    }
}
