using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Dark caption pill with a gold edge, placed under the belt so it never covers the rug.</summary>
    public class SpeechBubble : MonoBehaviour
    {
        public void Show(string text, Vector2 pos)
        {
            ViewKit.Clear(transform);
            if (string.IsNullOrEmpty(text)) { gameObject.SetActive(false); return; }
            gameObject.SetActive(true);
            transform.localPosition = pos;
            float width = Mathf.Clamp(3.4f + text.Length * 0.07f, 4.6f, 8.2f);
            ViewKit.Quad(transform, "Edge", Vector2.zero, new Vector2(width + 0.08f, 0.78f), ViewKit.Gold, 12, ArtKit.UiRound);
            ViewKit.Quad(transform, "Body", Vector2.zero, new Vector2(width, 0.66f), new Color(0.08f, 0.04f, 0.02f, 0.9f), 13, ArtKit.UiRound);
            ViewKit.Label(transform, "Caption", Vector2.zero, text, 0.032f, Color.white, 14);
        }

        public void Hide()
        {
            ViewKit.Clear(transform);
            gameObject.SetActive(false);
        }
    }
}
