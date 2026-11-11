using System;
using Rugtopia.Meta;
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
        Text _winTitle, _winCoins, _failTitle, _failLeft, _failBody, _continueLabel, _menuBody, _introText, _badgeText;
        Image _failFill;
        GameObject _badge;
        Button _langEn, _langTr;
        Text _playLabel, _langLabel, _mapTitle, _mapMeta, _menuSub, _skip, _chipText;
        GameObject _dim, _chip;
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

            _top = Panel(root, "TopBar", new Vector2(0, 0.885f), new Vector2(1, 1), new Color(0.12f, 0.07f, 0.05f, 0.0f)).gameObject;
            var top = _top.transform;
            Btn(top, "Home", new Vector2(0.03f, 0.42f), new Vector2(0.15f, 0.96f), "‹", "home");
            Btn(top, "Restart", new Vector2(0.85f, 0.42f), new Vector2(0.97f, 0.96f), "↻", "restart");
            _title = Txt(top, "Title", new Vector2(0.16f, 0.5f), new Vector2(0.84f, 1f), "", 58, TextAnchor.MiddleCenter, ViewKit.Cream);
            _status = Txt(top, "Status", new Vector2(0.2f, 0.16f), new Vector2(0.8f, 0.52f), "", 34, TextAnchor.MiddleCenter, ViewKit.Gold);
            _lives = Txt(top, "Lives", new Vector2(0.03f, 0.0f), new Vector2(0.2f, 0.38f), "", 28, TextAnchor.MiddleLeft, new Color(1f, 0.55f, 0.6f));
            _coins = Txt(top, "Coins", new Vector2(0.8f, 0.0f), new Vector2(0.97f, 0.38f), "", 28, TextAnchor.MiddleRight, ViewKit.Gold);

            _slot = Btn(root, "Slot", new Vector2(0.04f, 0.225f), new Vector2(0.32f, 0.305f), "+  Slot", "slot"); _slotL = _slot.GetComponentInChildren<Text>();
            _comb = Btn(root, "Comb", new Vector2(0.36f, 0.225f), new Vector2(0.64f, 0.305f), "#  Comb", "comb"); _combL = _comb.GetComponentInChildren<Text>();
            _shuttle = Btn(root, "Shuttle", new Vector2(0.68f, 0.225f), new Vector2(0.96f, 0.305f), ">  Shuttle", "shuttle"); _shuttleL = _shuttle.GetComponentInChildren<Text>();

            _dim = Panel(root, "Dim", Vector2.zero, Vector2.one, new Color(0.059f, 0.031f, 0.02f, 0.72f)).gameObject;
            _dim.SetActive(false);
            var chip = Panel(root, "CoinChip", new Vector2(0.33f, 0.905f), new Vector2(0.67f, 0.97f), new Color(0f, 0f, 0f, 0.55f));
            var chipImg = chip.GetComponent<Image>();
            chipImg.sprite = ArtKit.UiRound;
            chipImg.type = Image.Type.Sliced;
            var icon = Panel(chip, "Icon", new Vector2(0.06f, 0.14f), new Vector2(0.28f, 0.86f), Color.white);
            var iconImg = icon.GetComponent<Image>();
            iconImg.sprite = ArtKit.Coin;
            iconImg.raycastTarget = false;
            _chipText = Txt(chip, "Val", new Vector2(0.28f, 0f), new Vector2(0.96f, 1f), "", 34, TextAnchor.MiddleCenter, ViewKit.Hex("#FFE9A8"));
            _chip = chip.gameObject;
            _chip.SetActive(false);

            _winPanel = Card(root, "WinPanel", new Vector2(0.1f, 0.34f), new Vector2(0.9f, 0.68f), ViewKit.Green);
            _winTitle = Txt(_winPanel.transform, "Title", new Vector2(0.06f, 0.74f), new Vector2(0.94f, 0.96f), "", 72, TextAnchor.MiddleCenter, ViewKit.Ink);
            _stars = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                float x = 0.26f + i * 0.18f;
                _stars[i] = Txt(_winPanel.transform, "Star" + i, new Vector2(x, 0.52f), new Vector2(x + 0.16f, 0.74f), "★", 110, TextAnchor.MiddleCenter, ViewKit.Gold);
            }
            var coin = Panel(_winPanel.transform, "Coin", new Vector2(0.3f, 0.38f), new Vector2(0.42f, 0.52f), Color.white);
            var coinImg = coin.GetComponent<Image>();
            coinImg.sprite = ArtKit.Coin;
            coinImg.color = Color.white;
            coinImg.raycastTarget = false;
            _winCoins = Txt(_winPanel.transform, "Coins", new Vector2(0.42f, 0.36f), new Vector2(0.78f, 0.54f), "", 48, TextAnchor.MiddleLeft, ViewKit.Ink);
            var winNext = Btn(_winPanel.transform, "NextBtn", new Vector2(0.1f, 0.16f), new Vector2(0.9f, 0.38f), "Continue", "next", true);
            _skip = Txt(_winPanel.transform, "Skip", new Vector2(0.12f, 0.04f), new Vector2(0.88f, 0.14f), "", 26, TextAnchor.MiddleCenter, ViewKit.Wool);
            _langEn = Btn(_winPanel.transform, "LangEn", new Vector2(0.1f, 0.40f), new Vector2(0.46f, 0.52f), "English", "langEn");
            _langTr = Btn(_winPanel.transform, "LangTr", new Vector2(0.54f, 0.40f), new Vector2(0.9f, 0.52f), "Türkçe", "langTr", true);
            _langEn.gameObject.SetActive(false);
            _langTr.gameObject.SetActive(false);
            _badge = Panel(root, "Badge", new Vector2(0.18f, 0.80f), new Vector2(0.82f, 0.88f), ViewKit.Hex("#8E44AD")).gameObject;
            _badge.GetComponent<Image>().sprite = ArtKit.UiRound;
            _badge.GetComponent<Image>().type = Image.Type.Sliced;
            _badgeText = Txt(_badge.transform, "Label", Vector2.zero, Vector2.one, "", 40, TextAnchor.MiddleCenter, Color.white);
            _badge.SetActive(false);
            _playLabel = null;
            Remember(winNext, "winNext");

            _failPanel = Card(root, "FailPanel", new Vector2(0.1f, 0.32f), new Vector2(0.9f, 0.7f), ViewKit.Hex("#C0392B"));
            _failTitle = Txt(_failPanel.transform, "Title", new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.96f), "", 48, TextAnchor.MiddleCenter, ViewKit.Ink);
            _failLeft = Txt(_failPanel.transform, "Left", new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.78f), "", 36, TextAnchor.MiddleCenter, ViewKit.Ink);
            var bar = Panel(_failPanel.transform, "Bar", new Vector2(0.12f, 0.54f), new Vector2(0.88f, 0.62f), new Color(0.2f, 0.14f, 0.1f, 0.25f));
            _failFill = Panel(bar, "Fill", Vector2.zero, new Vector2(0.4f, 1f), ViewKit.Green).GetComponent<Image>();
            _failBody = Txt(_failPanel.transform, "Body", new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.54f), "", 28, TextAnchor.MiddleCenter, ViewKit.Wool);
            _continueLabel = Btn(_failPanel.transform, "ContinueBtn", new Vector2(0.1f, 0.28f), new Vector2(0.9f, 0.46f), "Continue", "continue", true).GetComponentInChildren<Text>();
            Btn(_failPanel.transform, "RetryBtn", new Vector2(0.08f, 0.06f), new Vector2(0.46f, 0.24f), "Retry", "retry");
            Btn(_failPanel.transform, "MapBtn", new Vector2(0.54f, 0.06f), new Vector2(0.92f, 0.24f), "Map", "map");

            _menu = Panel(root, "Menu", Vector2.zero, Vector2.one, ViewKit.Background).gameObject;
            Txt(_menu.transform, "Title", new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.82f), "Rugtopia", 92, TextAnchor.MiddleCenter, ViewKit.Cream);
            _menuSub = Txt(_menu.transform, "Sub", new Vector2(0.1f, 0.52f), new Vector2(0.9f, 0.62f), "", 40, TextAnchor.MiddleCenter, ViewKit.Gold);
            _menuBody = Txt(_menu.transform, "Body", new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.52f), "", 34, TextAnchor.MiddleCenter, ViewKit.Cream);
            _playLabel = Btn(_menu.transform, "Play", new Vector2(0.18f, 0.24f), new Vector2(0.82f, 0.36f), "Play", "play", true).GetComponentInChildren<Text>();
            _langLabel = Btn(_menu.transform, "Lang", new Vector2(0.28f, 0.12f), new Vector2(0.72f, 0.21f), "Türkçe", "lang").GetComponentInChildren<Text>();

            _intro = Card(root, "Intro", new Vector2(0.1f, 0.34f), new Vector2(0.9f, 0.66f), ViewKit.Gold);
            _introText = Txt(_intro.transform, "Body", new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.92f), "", 36, TextAnchor.MiddleCenter, ViewKit.Ink);
            Btn(_intro.transform, "Ok", new Vector2(0.22f, 0.08f), new Vector2(0.78f, 0.30f), "OK", "introOk", true);
            _intro.SetActive(false);

            _map = Panel(root, "Map", Vector2.zero, Vector2.one, ViewKit.Background).gameObject;
            _mapTitle = Txt(_map.transform, "Title", new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.98f), "Levels", 64, TextAnchor.MiddleCenter, ViewKit.Cream);
            Btn(_map.transform, "Back", new Vector2(0.04f, 0.91f), new Vector2(0.22f, 0.98f), "Back", "home");
            _mapMeta = Txt(_map.transform, "Meta", new Vector2(0.22f, 0.90f), new Vector2(0.98f, 0.98f), "", 28, TextAnchor.MiddleRight, ViewKit.Cream);
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
            if (_winTitle != null) _winTitle.text = Copy.Get("winTitle");
            SetChildLabel(_winPanel, "winNext", Copy.Get("next"));
            if (_skip != null) _skip.text = Copy.Get("tapSkip");
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
            _menuBody.text = "♥ " + Economy.Lives + "   ·   " + coins + " " + Copy.Get("coins");
        }

        public void ShowMap(int unlockedIndex, int count)
        {
            SetPlayChrome(false);
            HideIntro();
            ApplyLanguage();
            _menu.SetActive(false);
            _map.SetActive(true);
            HidePanels();
            if (_mapMeta != null) _mapMeta.text = "♥ " + Economy.Lives + "   ·   " + SaveService.Data.coins;
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
            if (_lives != null) _lives.gameObject.SetActive(false);
            if (_coins != null) _coins.gameObject.SetActive(false);
            if (_status != null) _status.gameObject.SetActive(false);
        }

        void SetPlayChrome(bool on)
        {
            if (_top != null) _top.SetActive(on);
            SetBoosters(on);
        }

        public void SetBoosters(bool on)
        {
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
            ShowDim(true);
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

        public void ShowLangChoice(bool on)
        {
            if (_langEn != null) _langEn.gameObject.SetActive(on);
            if (_langTr != null) _langTr.gameObject.SetActive(on);
        }

        public void ShowBadge(string text, Color color)
        {
            if (_badge == null) return;
            _badge.SetActive(true);
            var img = _badge.GetComponent<Image>();
            if (img != null) img.color = color;
            if (_badgeText != null) _badgeText.text = text;
            _badge.transform.SetAsLastSibling();
        }

        public void HideBadge() { if (_badge != null) _badge.SetActive(false); }

        public void ApplyTheme()
        {
            if (_title != null) _title.color = ThemeConfig.Ink;
            if (_status != null) _status.color = ThemeConfig.Muted;
        }

        public void ShowWin(string text) { ShowLevelComplete(0); if (_winTitle != null) _winTitle.text = text; }
        public void ShowFail(string title, int stitchesLeft, float progress, string continueLabel)
        {
            HidePanels();
            ApplyLanguage();
            _failTitle.text = title;
            if (_failLeft != null) _failLeft.text = stitchesLeft + " " + Copy.Get(stitchesLeft == 1 ? "left1" : "leftN");
            if (_failFill != null)
            {
                var rt = _failFill.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            _failBody.text = "";
            _continueLabel.text = continueLabel;
            ShowDim(false);
            _failPanel.SetActive(true);
            _failPanel.transform.SetAsLastSibling();
        }
        public void HidePanels()
        {
            _winPanel.SetActive(false);
            _failPanel.SetActive(false);
            if (_dim != null) _dim.SetActive(false);
            if (_chip != null) _chip.SetActive(false);
        }

        void ShowDim(bool withChip)
        {
            if (_dim != null)
            {
                _dim.SetActive(true);
                _dim.transform.SetAsLastSibling();
            }
            if (_chip != null)
            {
                _chip.SetActive(withChip);
                if (withChip && _chipText != null) _chipText.text = _coins != null ? _coins.text.Replace(" ¢", "") : "";
                if (withChip) _chip.transform.SetAsLastSibling();
            }
        }

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
        GameObject Card(Transform parent, string name, Vector2 aMin, Vector2 aMax, Color border)
        {
            var edge = Panel(parent, name, aMin, aMax, border);
            var edgeImg = edge.GetComponent<Image>();
            edgeImg.sprite = ArtKit.UiRound;
            edgeImg.type = Image.Type.Sliced;
            var fill = Panel(edge, "Fill", new Vector2(0.025f, 0.02f), new Vector2(0.975f, 0.98f), ViewKit.Hex("#F3E6CF"));
            var fillImg = fill.GetComponent<Image>();
            fillImg.sprite = ArtKit.UiRound;
            fillImg.type = Image.Type.Sliced;
            fillImg.raycastTarget = false;
            return edge.gameObject;
        }
        static Text Txt(Transform parent, string name, Vector2 aMin, Vector2 aMax, string s, int size, TextAnchor anchor, Color c)
        {
            var t = Rect(parent, name, aMin, aMax).gameObject.AddComponent<Text>();
            t.font = ViewKit.Font; t.text = s; t.fontSize = size; t.fontStyle = FontStyle.Bold; t.alignment = anchor; t.color = c;
            t.alignByGeometry = true; t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }
        Button Btn(Transform parent, string name, Vector2 aMin, Vector2 aMax, string label, string id)
        {
            return Btn(parent, name, aMin, aMax, label, id, false);
        }

        Button Btn(Transform parent, string name, Vector2 aMin, Vector2 aMax, string label, string id, bool primary)
        {
            var rt = Panel(parent, name, aMin, aMax, primary ? ViewKit.Green : ViewKit.Hex("#3D2A20"));
            var image = rt.GetComponent<Image>();
            image.sprite = ArtKit.UiRound;
            image.type = Image.Type.Sliced;
            var b = rt.gameObject.AddComponent<Button>();
            var ink = primary ? Color.white : ViewKit.Cream;
            var text = Txt(rt, "Label", Vector2.zero, Vector2.one, label, 44, TextAnchor.MiddleCenter, ink);
            var lrt = text.rectTransform;
            lrt.anchorMin = new Vector2(0.06f, 0.5f);
            lrt.anchorMax = new Vector2(0.94f, 0.5f);
            lrt.pivot = new Vector2(0.5f, 0.5f);
            lrt.sizeDelta = new Vector2(0f, 72f);
            lrt.anchoredPosition = Vector2.zero;
            b.onClick.AddListener(() => { if (Clicked != null) Clicked(id); });
            return b;
        }
    }
}
