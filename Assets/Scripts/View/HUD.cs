using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Rugtopia.View
{
    /// <summary>Screen-space UI. Top bar holds the stitch counter. Win and fail cards share one tight layout.</summary>
    public class HUD : MonoBehaviour
    {
        public event Action<string> Clicked;

        Canvas _canvas;
        Text _title, _coins, _lives, _status;
        GameObject _winPanel, _failPanel, _menu, _map, _top;
        Text _winTitle, _winCoins, _failTitle, _failBody, _continueLabel, _menuBody, _introText;
        Text _playLabel, _langLabel, _mapTitle, _menuSub;
        Text[] _stars;
        GameObject _intro;
        Button _slot, _comb, _shuttle;
        Text _slotL, _combL, _shuttleL;
        Button[] _levels;
        Text[] _levelLabels;
        float _starT;

        public void Build()
        {
            var canvasGo = new GameObject("HUD Canvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
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

            _top = Panel(root, "TopBar", new Vector2(0, 0.905f), new Vector2(1, 1), new Color(0.29f, 0.17f, 0.12f, 0.96f)).gameObject;
            var top = _top.transform;
            _title = Txt(top, "Title", new Vector2(0.32f, 0.46f), new Vector2(0.70f, 1f), "", 36, TextAnchor.MiddleCenter, Color.white);
            _status = Txt(top, "Status", new Vector2(0.30f, 0.02f), new Vector2(0.72f, 0.48f), "", 28, TextAnchor.MiddleCenter, ViewKit.Hex("#F5B700"));
            _coins = Txt(top, "Coins", new Vector2(0.76f, 0), new Vector2(0.98f, 1), "", 34, TextAnchor.MiddleRight, ViewKit.Hex("#F5B700"));
            _lives = Txt(top, "Lives", new Vector2(0.02f, 0), new Vector2(0.18f, 1), "", 34, TextAnchor.MiddleLeft, new Color(1f, 0.55f, 0.6f));
            Btn(top, "Home", new Vector2(0.18f, 0.18f), new Vector2(0.31f, 0.82f), "Menu", "home");
            Btn(top, "Next", new Vector2(0.70f, 0.18f), new Vector2(0.76f, 0.82f), ">", "next");

            _slot = Btn(root, "Slot", new Vector2(0.05f, 0.355f), new Vector2(0.33f, 0.425f), "+  Slot", "slot"); _slotL = _slot.GetComponentInChildren<Text>();
            _comb = Btn(root, "Comb", new Vector2(0.36f, 0.355f), new Vector2(0.64f, 0.425f), "#  Comb", "comb"); _combL = _comb.GetComponentInChildren<Text>();
            _shuttle = Btn(root, "Shuttle", new Vector2(0.67f, 0.355f), new Vector2(0.95f, 0.425f), ">  Shuttle", "shuttle"); _shuttleL = _shuttle.GetComponentInChildren<Text>();

            _winPanel = Card(root, "WinPanel", new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.64f));
            _winTitle = Txt(_winPanel.transform, "Title", new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.96f), "", 46, TextAnchor.MiddleCenter, ViewKit.Ink);
            _stars = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                float x = 0.30f + i * 0.16f;
                _stars[i] = Txt(_winPanel.transform, "Star" + i, new Vector2(x, 0.50f), new Vector2(x + 0.12f, 0.72f), "★", 64, TextAnchor.MiddleCenter, ViewKit.Hex("#F5B700"));
            }
            var coin = Panel(_winPanel.transform, "Coin", new Vector2(0.28f, 0.34f), new Vector2(0.40f, 0.50f), Color.white);
            var coinImg = coin.GetComponent<Image>();
            coinImg.sprite = ArtKit.Coin;
            coinImg.color = Color.white;
            coinImg.raycastTarget = false;
            _winCoins = Txt(_winPanel.transform, "Coins", new Vector2(0.40f, 0.32f), new Vector2(0.82f, 0.52f), "", 36, TextAnchor.MiddleLeft, ViewKit.Ink);
            var winMap = Btn(_winPanel.transform, "MapBtn", new Vector2(0.08f, 0.07f), new Vector2(0.46f, 0.28f), "Map", "map");
            var winNext = Btn(_winPanel.transform, "NextBtn", new Vector2(0.54f, 0.07f), new Vector2(0.92f, 0.28f), "Next", "next", true);
            _playLabel = null;
            Remember(winMap, "winMap");
            Remember(winNext, "winNext");

            _failPanel = Card(root, "FailPanel", new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.66f));
            _failTitle = Txt(_failPanel.transform, "Title", new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.96f), "", 44, TextAnchor.MiddleCenter, ViewKit.Ink);
            _failBody = Txt(_failPanel.transform, "Body", new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.72f), "", 30, TextAnchor.MiddleCenter, ViewKit.Wool);
            _continueLabel = Btn(_failPanel.transform, "ContinueBtn", new Vector2(0.1f, 0.28f), new Vector2(0.9f, 0.46f), "Continue", "continue", true).GetComponentInChildren<Text>();
            Btn(_failPanel.transform, "RetryBtn", new Vector2(0.08f, 0.06f), new Vector2(0.46f, 0.24f), "Retry", "retry");
            Btn(_failPanel.transform, "MapBtn", new Vector2(0.54f, 0.06f), new Vector2(0.92f, 0.24f), "Map", "map");

            _menu = Panel(root, "Menu", Vector2.zero, Vector2.one, ViewKit.Background).gameObject;
            Txt(_menu.transform, "Title", new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.82f), "Rugtopia", 92, TextAnchor.MiddleCenter, ViewKit.Ink);
            _menuSub = Txt(_menu.transform, "Sub", new Vector2(0.1f, 0.52f), new Vector2(0.9f, 0.62f), "", 40, TextAnchor.MiddleCenter, ViewKit.Wool);
            _menuBody = Txt(_menu.transform, "Body", new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.52f), "", 34, TextAnchor.MiddleCenter, ViewKit.Ink);
            _playLabel = Btn(_menu.transform, "Play", new Vector2(0.18f, 0.24f), new Vector2(0.82f, 0.36f), "Play", "play", true).GetComponentInChildren<Text>();
            _langLabel = Btn(_menu.transform, "Lang", new Vector2(0.28f, 0.12f), new Vector2(0.72f, 0.21f), "Türkçe", "lang").GetComponentInChildren<Text>();

            _intro = Card(root, "Intro", new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.64f));
            _introText = Txt(_intro.transform, "Body", new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.92f), "", 36, TextAnchor.MiddleCenter, ViewKit.Ink);
            Btn(_intro.transform, "Ok", new Vector2(0.22f, 0.08f), new Vector2(0.78f, 0.30f), "OK", "introOk", true);
            _intro.SetActive(false);

            _map = Panel(root, "Map", Vector2.zero, Vector2.one, ViewKit.Background).gameObject;
            _mapTitle = Txt(_map.transform, "Title", new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.98f), "Levels", 64, TextAnchor.MiddleCenter, ViewKit.Ink);
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
            ApplyLanguage();
            ShowMenu(0, 1);
        }

        void Remember(Button b, string key)
        {
            if (b == null) return;
            b.gameObject.name = key;
        }

        public void ApplyLanguage()
        {
            if (_menuSub != null) _menuSub.text = Copy.Get("menuSub");
            if (_playLabel != null) _playLabel.text = Copy.Get("play");
            if (_langLabel != null) _langLabel.text = Copy.Get(LangIsTr() ? "langToEn" : "langToTr");
            if (_mapTitle != null) _mapTitle.text = Copy.Get("levels");
            if (_winTitle != null) _winTitle.text = Copy.Get("levelComplete");
            SetChildLabel(_winPanel, "winMap", Copy.Get("levels"));
            string next = Copy.Get("nextLevel");
            if (next == "nextLevel") next = LangIsTr() ? "Sonraki" : "Next";
            SetChildLabel(_winPanel, "winNext", next);
            SetChildLabel(_failPanel, "RetryBtn", Copy.Get("retryFree"));
            SetChildLabel(_failPanel, "MapBtn", Copy.Get("levels"));
        }

        static bool LangIsTr() { return SaveServiceLang() == "tr"; }
        static string SaveServiceLang()
        {
            var d = Rugtopia.Meta.SaveService.Data;
            return d != null && d.lang == "tr" ? "tr" : "en";
        }

        static void SetChildLabel(GameObject panel, string child, string text)
        {
            if (panel == null) return;
            var t = panel.transform.Find(child);
            if (t == null) return;
            var label = t.GetComponentInChildren<Text>();
            if (label != null) label.text = text;
        }

        public void ShowMenu(int coins, int reached)
        {
            SetPlayChrome(false);
            HideIntro();
            ApplyLanguage();
            _menu.SetActive(true);
            _map.SetActive(false);
            HidePanels();
            _menuBody.text = coins + " " + Copy.Get("coins") + " · " + reached;
        }

        public void ShowMap(int unlockedIndex, int count)
        {
            SetPlayChrome(false);
            HideIntro();
            ApplyLanguage();
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
                if (img != null) img.color = open ? ViewKit.Hex("#F6ECD9") : new Color(0.72f, 0.66f, 0.58f);
                if (_levelLabels[i] != null) _levelLabels[i].text = open ? (i + 1).ToString() : "·";
            }
        }

        public void Bind(Camera cam)
        {
            if (_canvas == null || cam == null) return;
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = cam;
            _canvas.planeDistance = 2f;
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
            if (_slot != null) _slot.gameObject.SetActive(on);
            if (_comb != null) _comb.gameObject.SetActive(on);
            if (_shuttle != null) _shuttle.gameObject.SetActive(on);
        }

        public void ShowIntro(string text)
        {
            if (_intro == null) return;
            _introText.text = text;
            _intro.SetActive(true);
            _intro.transform.SetAsLastSibling();
        }

        public void HideIntro()
        {
            if (_intro != null) _intro.SetActive(false);
        }

        public void ShowLevelComplete(int coins)
        {
            bool restartStars = _winPanel == null || !_winPanel.activeSelf;
            HidePanels();
            HideIntro();
            ApplyLanguage();
            if (restartStars) _starT = 0f;
            _winCoins.text = "+" + coins + "  " + Copy.Get("coins");
            _winPanel.SetActive(true);
            _winPanel.transform.SetAsLastSibling();
        }

        void Update()
        {
            if (_stars == null || _winPanel == null || !_winPanel.activeSelf) return;
            _starT += Time.deltaTime;
            for (int i = 0; i < _stars.Length; i++)
            {
                float pop = Mathf.Clamp01((_starT - i * 0.12f) / 0.28f);
                float bounce = pop < 1f ? Mathf.Sin(pop * Mathf.PI) * 0.35f : 0.08f * Mathf.Sin(Time.time * 5f + i);
                float s = Mathf.Lerp(0.2f, 1f, pop) + bounce;
                _stars[i].rectTransform.localScale = new Vector3(s, s, 1f);
            }
        }

        public void SetTop(string title, int coins, int lives) { _title.text = title; _coins.text = coins + " ¢"; _lives.text = "♥ " + lives; }
        public void SetStatus(string s) { if (_status != null) _status.text = s; }

        public void SetBooster(string id, bool unlocked, int inventory, int price, bool armed)
        {
            Button b = id == "slot" ? _slot : id == "comb" ? _comb : _shuttle;
            Text l = id == "slot" ? _slotL : id == "comb" ? _combL : _shuttleL;
            string name = id == "slot" ? "+  Slot" : id == "comb" ? "#  Comb" : ">  Shuttle";
            b.interactable = unlocked;
            l.text = !unlocked ? name : name + (inventory > 0 ? "  x" + inventory : "  " + price) + (armed ? "  ON" : "");
        }

        public void ShowWin(string text) { ShowLevelComplete(0); if (_winTitle != null) _winTitle.text = text; }
        public void ShowFail(string title, string body, int continuePrice)
        {
            HidePanels();
            ApplyLanguage();
            _failTitle.text = title;
            _failBody.text = body;
            _continueLabel.text = Copy.Get("bSlot") + "  " + continuePrice;
            _failPanel.SetActive(true);
            _failPanel.transform.SetAsLastSibling();
        }
        public void HidePanels() { _winPanel.SetActive(false); _failPanel.SetActive(false); }

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
        GameObject Card(Transform parent, string name, Vector2 aMin, Vector2 aMax)
        {
            var rt = Panel(parent, name, aMin, aMax, ViewKit.Hex("#FFF8EE"));
            var image = rt.GetComponent<Image>();
            image.sprite = ArtKit.UiRound;
            image.type = Image.Type.Sliced;
            return rt.gameObject;
        }
        static Text Txt(Transform parent, string name, Vector2 aMin, Vector2 aMax, string s, int size, TextAnchor anchor, Color c)
        {
            var t = Rect(parent, name, aMin, aMax).gameObject.AddComponent<Text>();
            t.font = ViewKit.Font; t.text = s; t.fontSize = size; t.alignment = anchor; t.color = c;
            t.alignByGeometry = true; t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }
        Button Btn(Transform parent, string name, Vector2 aMin, Vector2 aMax, string label, string id)
        {
            return Btn(parent, name, aMin, aMax, label, id, false);
        }

        Button Btn(Transform parent, string name, Vector2 aMin, Vector2 aMax, string label, string id, bool primary)
        {
            var rt = Panel(parent, name, aMin, aMax, primary ? ViewKit.Hex("#C9343F") : ViewKit.Hex("#E7C99A"));
            var image = rt.GetComponent<Image>();
            image.sprite = ArtKit.UiRound;
            image.type = Image.Type.Sliced;
            var b = rt.gameObject.AddComponent<Button>();
            var ink = primary ? Color.white : ViewKit.Ink;
            var text = Txt(rt, "Label", Vector2.zero, Vector2.one, label, 32, TextAnchor.MiddleCenter, ink);
            var lrt = text.rectTransform;
            lrt.anchorMin = new Vector2(0.06f, 0.5f);
            lrt.anchorMax = new Vector2(0.94f, 0.5f);
            lrt.pivot = new Vector2(0.5f, 0.5f);
            lrt.sizeDelta = new Vector2(0f, 48f);
            lrt.anchoredPosition = Vector2.zero;
            b.onClick.AddListener(() => { if (Clicked != null) Clicked(id); });
            return b;
        }
    }
}
