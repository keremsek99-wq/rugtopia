using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Large tutorial line: dark fill, gold border, bold type. Sits under the belt.</summary>
    public class SpeechBubble : MonoBehaviour
    {
        public void Show(string text, Vector2 pos)
        {
            ViewKit.Clear(transform);
            if (string.IsNullOrEmpty(text)) { gameObject.SetActive(false); return; }
            gameObject.SetActive(true);
            transform.localPosition = pos;
            float width = 8.4f;
            ViewKit.Quad(transform, "Edge", Vector2.zero, new Vector2(width + 0.14f, 1.05f), ViewKit.Gold, 12, ArtKit.UiRound);
            ViewKit.Quad(transform, "Body", Vector2.zero, new Vector2(width, 0.88f), new Color(0.08f, 0.04f, 0.02f, 0.92f), 13, ArtKit.UiRound);
            ViewKit.Label(transform, "Caption", Vector2.zero, text, 0.09f, Color.white, 14);
        }

        public void Hide()
        {
            ViewKit.Clear(transform);
            gameObject.SetActive(false);
        }
    }
}
