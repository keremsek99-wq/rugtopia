using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>One-line tutorial caption in a bubble under the rug, not on top of the weave.</summary>
    public class SpeechBubble : MonoBehaviour
    {
        public void Show(string text, Vector2 pos)
        {
            ViewKit.Clear(transform);
            if (string.IsNullOrEmpty(text)) { gameObject.SetActive(false); return; }
            gameObject.SetActive(true);
            transform.localPosition = pos;
            float width = Mathf.Clamp(2.2f + text.Length * 0.105f, 4.2f, 8.3f);
            ViewKit.Quad(transform, "Body", Vector2.zero, new Vector2(width, 1.15f), ViewKit.Hex("#FFF8EE"), 12, ArtKit.UiRound);
            var tail = ViewKit.Quad(transform, "Tail", new Vector2(0f, 0.68f), new Vector2(0.34f, 0.34f), ViewKit.Hex("#FFF8EE"), 12, ArtKit.Card);
            tail.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            ViewKit.Label(transform, "Caption", Vector2.zero, text, 0.034f, ViewKit.Ink, 13);
        }

        public void Hide()
        {
            ViewKit.Clear(transform);
            gameObject.SetActive(false);
        }
    }
}
