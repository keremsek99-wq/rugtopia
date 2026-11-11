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
        GameObject _winPanel, _failPanel, _menu, _map, _top;
        Text _winText, _failText, _continueLabel, _menuBody;
        Button _slot, _comb, _shuttle;
        Text _slotL, _combL, _shuttleL;
        Button[] _levels;
        Text[] _levelLabels;

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

            _top = Panel(root, "TopBar", new Vector2(0, 0.93f), new Vector2(1, 1), new Color(0.29f, 0.17f, 0.12f, 0.92f)).gameObject;
            var top = _top.transform;
            _title = Txt(top, "Title", new Vector2(0.36f, 0), new Vector2(0.68f, 1), "", 40, TextAnchor.MiddleCenter, Color.white);
            _coins = Txt(top, "Coins", new Vector2(0.78f, 0), new Vector2(0.98f, 1), "", 38, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.3f));
            _lives = Txt(top, "Lives", new Vector2(0.02f, 0), new Vector2(0.22f, 1), "", 38, TextAnchor.MiddleLeft, new Color(1f, 0.55f, 0.6f));
            Btn(top, "Home", new Vector2(0.24f, 0.15f), new Vector2(0.34f, 0.85f), "Menu", "home");
            Btn(top, "Next", new Vector2(0.7f, 0.15f), new Vector2(0.76f, 0.85f), ">", "next");
            _status = Txt(root, "Status", new Vector2(0.05f, 0.885f), new Vector2(0.95f, 0.925f), "", 32, TextAnchor.MiddleCenter, new Color(0.17f, 0.1f, 0.08f));

            _slot = Btn(root, "Slot", new Vector2(0.06f, 0.365f), new Vector2(0.34f, 0.41f), "+1 Slot", "slot"); _slotL = _slot.GetComponentInChildren<Text>();
            _comb = Btn(root, "Comb", new Vector2(0.36f, 0.365f), new Vector2(0.64f, 0.41f), "Comb", "comb"); _combL = _comb.GetComponentInChildren<Text>();
            _shuttle = Btn(root, "Shuttle", new Vector2(0.66f, 0.365f), new Vector2(0.94f, 0.41f), "Shuttle", "shuttle"); _shuttleL = _shuttle.GetComponentInChildren<Text>();

            _winPanel = Panel(root, "WinPanel", new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f), new Color(0.2f, 0.45f, 0.3f, 0.96f)).gameObject;
            _winText = Txt(_winPanel.transform, "Text", new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.95f), "", 52, TextAnchor.MiddleCenter, Color.white);
            Btn(_winPanel.transform, "MapBtn", new Vector2(0.08f, 0.08f), new Vector2(0.46f, 0.38f), "Map", "map");
            Btn(_winPanel.transform, "NextBtn", new Vector2(0.54f, 0.08f), new Vector2(0.92f, 0.38f), "Next", "next");
            _failPanel = Panel(root, "FailPanel", new Vector2(0.1f, 0.33f), new Vector2(0.9f, 0.67f), new Color(0.55f, 0.18f, 0.2f, 0.96f)).gameObject;
            _failText = Txt(_failPanel.transform, "Text", new Vector2(0.05f, 0.6f), new Vector2(0.95f, 0.95f), "", 48, TextAnchor.MiddleCenter, Color.white);
            _continueLabel = Btn(_failPanel.transform, "ContinueBtn", new Vector2(0.15f, 0.33f), new Vector2(0.85f, 0.55f), "Continue", "continue").GetComponentInChildren<Text>();
            Btn(_failPanel.transform, "RetryBtn", new Vector2(0.08f, 0.06f), new Vector2(0.48f, 0.28f), "Retry", "retry");
            Btn(_failPanel.transform, "MapBtn", new Vector2(0.52f, 0.06f), new Vector2(0.92f, 0.28f), "Map", "map");

            _menu = Panel(root, "Menu", Vector2.zero, Vector2.one, ViewKit.Background).gameObject;
            Txt(_menu.transform, "Title", new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.82f), "Rugtopia", 92, TextAnchor.MiddleCenter, ViewKit.Ink);
            Txt(_menu.transform, "Sub", new Vector2(0.1f, 0.52f), new Vector2(0.9f, 0.62f), "Weave the rug", 40, TextAnchor.MiddleCenter, ViewKit.Wool);
            _menuBody = Txt(_menu.transform, "Body", new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.5f), "", 34, TextAnchor.MiddleCenter, ViewKit.Ink);
            Btn(_menu.transform, "Play", new Vector2(0.18f, 0.22f), new Vector2(0.82f, 0.34f), "Play", "play");

            _map = Panel(root, "Map", Vector2.zero, Vector2.one, ViewKit.Background).gameObject;
            Txt(_map.transform, "Title", new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.98f), "Levels", 64, TextAnchor.MiddleCenter, ViewKit.Ink);
            Btn(_map.transform, "Back", new Vector2(0.04f, 0.91f), new Vector2(0.22f, 0.98f), "Back", "home");
            int n = 30;
            _levels = new Button[n];
            _levelLabels = new Text[n];
            for (int i = 0; i < n; i++)
            {
                int col = i % 5, row = i / 5;
                float x0 = 0.05f + col * 0.185f;
                float y1 = 0.84f - row * 0.11f;
                _levels[i] = Btn(_map.transform, "L" + i, new Vector2(x0, y1 - 0.09f), new Vector2(x0 + 0.16f, y1), (i + 1).ToString(), "lv" + i);
                _levelLabels[i] = _levels[i].GetComponentInChildren<Text>();
            }
            HidePanels();
            ShowMenu(0, 1);
        }

        public void ShowMenu(int coins, int reached)
        {
            SetPlayChrome(false);
            _menu.SetActive(true);
            _map.SetActive(false);
            HidePanels();
            _menuBody.text = coins + " coins · " + reached + " open";
        }

        public void ShowMap(int unlockedIndex, int count)
        {
            SetPlayChrome(false);
            _menu.SetActive(false);
            _map.SetActive(true);
            HidePanels();
            int n = _levels == null ? 0 : _levels.Length;
            for (int i = 0; i < n; i++)
            {
                bool open = i < count && i <= unlockedIndex;
                _levels[i].gameObject.SetActive(i < count);
                _levels[i].interactable = open;
                var img = _levels[i].GetComponent<Image>();
                if (img != null) img.color = open ? new Color(0.96f, 0.9f, 0.78f) : new Color(0.72f, 0.66f, 0.58f);
                if (_levelLabels[i] != null) _levelLabels[i].text = open ? (i + 1).ToString() : "·";
            }
        }

        public void ShowPlay()
        {
            SetPlayChrome(true);
            if (_menu != null) _menu.SetActive(false);
            if (_map != null) _map.SetActive(false);
        }

        void SetPlayChrome(bool on)
        {
            if (_top != null) _top.SetActive(on);
            if (_status != null) _status.gameObject.SetActive(on);
            if (_slot != null) _slot.gameObject.SetActive(on);
            if (_comb != null) _comb.gameObject.SetActive(on);
            if (_shuttle != null) _shuttle.gameObject.SetActive(on);
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
