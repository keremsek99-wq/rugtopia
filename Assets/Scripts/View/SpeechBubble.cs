using UnityEngine;

namespace Rugtopia.View
{
    /// <summary>Tutorial line in a gold-edged pill sized to the text, wrapping onto a second line when needed.</summary>
    public class SpeechBubble : MonoBehaviour
    {
        public void Show(string text, Vector2 pos)
        {
            ViewKit.Clear(transform);
            if (string.IsNullOrEmpty(text)) { gameObject.SetActive(false); return; }
            gameObject.SetActive(true);
            transform.localPosition = pos;
            string shown = Wrap(text, 28);
            int lines = shown.IndexOf('\n') >= 0 ? 2 : 1;
            int longest = Longest(shown);
            // TextMesh advance is wider than characterSize, so the pill is sized from that.
            float width = Mathf.Clamp(longest * 0.155f + 0.9f, 4.4f, 8.5f);
            float height = lines == 2 ? 1.35f : 0.95f;
            ViewKit.Quad(transform, "Edge", Vector2.zero, new Vector2(width + 0.16f, height + 0.16f), ThemeConfig.BubbleEdge, 12, ArtKit.UiRound);
            ViewKit.Quad(transform, "Body", Vector2.zero, new Vector2(width, height), ThemeConfig.Bubble, 13, ArtKit.UiRound);
            ViewKit.Label(transform, "Caption", Vector2.zero, shown, 0.085f, ThemeConfig.BubbleText, 14);
        }

        public void Hide()
        {
            ViewKit.Clear(transform);
            gameObject.SetActive(false);
        }

        static string Wrap(string text, int limit)
        {
            if (text.Length <= limit) return text;
            int mid = text.Length / 2;
            int sp = text.LastIndexOf(' ', mid + 6);
            if (sp < 8) sp = text.IndexOf(' ', mid);
            if (sp <= 0) return text;
            return text.Substring(0, sp) + "\n" + text.Substring(sp + 1);
        }

        static int Longest(string text)
        {
            int best = 0, run = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') { if (run > best) best = run; run = 0; }
                else run++;
            }
            return run > best ? run : best;
        }
    }
}
