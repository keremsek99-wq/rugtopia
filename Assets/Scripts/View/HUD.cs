using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Rugtopia.View
{
    /// <summary>Screen-space UI built in code (uGUI + legacy Text so it needs no TMP Essentials import in week 1).
    /// Top bar, booster buttons, win/fail panels. Strings are English placeholders; Unity Localization tables come in week 4.</summary>
    public class HUD : MonoBehaviour
    {
        public event Action<string> Clicked;   // ids: slot, comb, shuttle, continue, retry, next, prev, menu

        Text _title, _coins, _lives, _status;
        GameObject _winPanel, _failPanel;
        Text _winText, _failText, _continueLabel;
        Button _slot, _comb, _shuttle;
        Text _slotL, _combL, _shuttleL;

        public void Build()
        {
            var canvasGo = new GameObject("HUD Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2340); scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var root = canvasGo.GetComponent<RectTransform>();

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                es.AddComponent<InputSystemUIInputModule>();
#else
                es.AddComponent<StandaloneInputModule>();
#endif
            }

            var top = Panel(root, "TopBar", new Vector2(0, 0.93f), new Vector2(1, 1), new Color(0.29f, 0.17f, 0.12f, 0.92f));
            _title = Txt(top, "Title", new Vector2(0.22f, 0), new Vector2(0.78f, 1), "", 44, TextAnchor.MiddleCenter, Color.white);
            _coins = Txt(top, "Coins", new Vector2(0.78f, 0), new Vector2(0.98f, 1), "", 38, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.3f));
            _lives = Txt(top, "Lives", new Vector2(0.02f, 0), new Vector2(0.22f, 1), "", 38, TextAnchor.MiddleLeft, new Color(1f, 0.55f, 0.6f));
            Btn(top, "Prev", new Vector2(0.24f, 0.15f), new Vector2(0.3f, 0.85f), "<", "prev");
            Btn(top, "Next", new Vector2(0.7f, 0.15f), new Vector2(0.76f, 0.85f), ">", "next");
            _status = Txt(root, "Status", new Vector2(0.05f, 0.885f), new Vector2(0.95f, 0.925f), "", 32, TextAnchor.MiddleCenter, new Color(0.17f, 0.1f, 0.08f));

            _slot = Btn(root, "Slot", new Vector2(0.06f, 0.365f), new Vector2(0.34f, 0.41f), "+1 Slot", "slot"); _slotL = _slot.GetComponentInChildren<Text>();
            _comb = Btn(root, "Comb", new Vector2(0.36f, 0.365f), new Vector2(0.64f, 0.41f), "Comb", "comb"); _combL = _comb.GetComponentInChildren<Text>();
            _shuttle = Btn(root, "Shuttle", new Vector2(0.66f, 0.365f), new Vector2(0.94f, 0.41f), "Shuttle", "shuttle"); _shuttleL = _shuttle.GetComponentInChildren<Text>();

            _winPanel = Panel(root, "WinPanel", new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f), new Color(0.2f, 0.45f, 0.3f, 0.96f)).gameObject;
            _winText = Txt(_winPanel.transform, "Text", new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.95f), "", 52, TextAnchor.MiddleCenter, Color.white);
            Btn(_winPanel.transform, "NextBtn", new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.35f), "Next", "next");
            _failPanel = Panel(root, "FailPanel", new Vector2(0.1f, 0.33f), new Vector2(0.9f, 0.67f), new Color(0.55f, 0.18f, 0.2f, 0.96f)).gameObject;
            _failText = Txt(_failPanel.transform, "Text", new Vector2(0.05f, 0.6f), new Vector2(0.95f, 0.95f), "", 48, TextAnchor.MiddleCenter, Color.white);
            _continueLabel = Btn(_failPanel.transform, "ContinueBtn", new Vector2(0.15f, 0.33f), new Vector2(0.85f, 0.55f), "Continue", "continue").GetComponentInChildren<Text>();
            Btn(_failPanel.transform, "RetryBtn", new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.27f), "Retry", "retry");
            HidePanels();
        }

        public void SetTop(string title, int coins, int lives) { _title.text = title; _coins.text = coins + " ¢"; _lives.text = "♥ " + lives; }
        public void SetStatus(string s) { _status.text = s; }

        public void SetBooster(string id, bool unlocked, int inventory, int price, bool armed)
        {
            Button b = id == "slot" ? _slot : id == "comb" ? _comb : _shuttle;
            Text l = id == "slot" ? _slotL : id == "comb" ? _combL : _shuttleL;
            string name = id == "slot" ? "+1 Slot" : id == "comb" ? "Comb" : "Shuttle";
            b.interactable = unlocked;
            l.text = !unlocked ? name + " (locked)" : name + (inventory > 0 ? " x" + inventory : " " + price + "¢") + (armed ? " ON" : "");
        }

        public void ShowWin(string text) { HidePanels(); _winText.text = text; _winPanel.SetActive(true); }
        public void ShowFail(string text, int continuePrice) { HidePanels(); _failText.text = text; _continueLabel.text = "Continue +1 slot · " + continuePrice + "¢"; _failPanel.SetActive(true); }
        public void HidePanels() { _winPanel.SetActive(false); _failPanel.SetActive(false); }

        // ---- tiny uGUI builders ----
        static RectTransform Rect(Transform parent, string name, Vector2 aMin, Vector2 aMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return rt;
        }
        static RectTransform Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Color c)
        {
            var rt = Rect(parent, name, aMin, aMax); rt.gameObject.AddComponent<Image>().color = c; return rt;
        }
        static Text Txt(Transform parent, string name, Vector2 aMin, Vector2 aMax, string s, int size, TextAnchor anchor, Color c)
        {
            var t = Rect(parent, name, aMin, aMax).gameObject.AddComponent<Text>();
            t.font = ViewKit.Font; t.text = s; t.fontSize = size; t.alignment = anchor; t.color = c; t.raycastTarget = false;
            return t;
        }
        Button Btn(Transform parent, string name, Vector2 aMin, Vector2 aMax, string label, string id)
        {
            var rt = Panel(parent, name, aMin, aMax, new Color(0.96f, 0.9f, 0.78f));
            var b = rt.gameObject.AddComponent<Button>();
            Txt(rt, "Label", Vector2.zero, Vector2.one, label, 34, TextAnchor.MiddleCenter, new Color(0.17f, 0.1f, 0.08f));
            b.onClick.AddListener(() => { if (Clicked != null) Clicked(id); });
            return b;
        }
    }
}
