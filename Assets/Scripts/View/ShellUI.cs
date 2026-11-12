using System;
using System.Collections.Generic;
using Rugtopia.Core;
using Rugtopia.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Rugtopia.View
{
    /// <summary>Home, path, daily, streak and gallery. The play board is not drawn here.</summary>
    public class ShellUI : MonoBehaviour
    {
        public event Action<string> Clicked;

        GameObject _home, _path, _daily, _gallery, _streak, _settings;
        RectTransform _hero, _galleryStrip, _playBtn;
        Text _playSub, _livesText, _coinsText, _timerText, _dailyLock, _streakLock, _streakReward;
        Image[] _pips;
        Image[] _modalPips;
        Text _streakCount, _streakFoot;
        Image _dailyBadge;
        ScrollRect _scroll;
        RectTransform _pathContent;

        public void Build(Transform canvas)
        {
            _home = Screen(canvas, "Home");
            BuildHome(_home.transform);
            _path = Screen(canvas, "Path");
            BuildPath(_path.transform);
            _daily = Screen(canvas, "Daily");
            BuildDaily(_daily.transform);
            _gallery = Screen(canvas, "Gallery");
            BuildGallery(_gallery.transform);
            _streak = Screen(canvas, "Streak");
            BuildStreak(_streak.transform);
            _settings = Card(_home.transform, "Settings", new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.72f), ThemeConfig.Button);
            BuildSettings(_settings.transform);
            Hide();
        }

        public void Hide()
        {
            if (_home != null) _home.SetActive(false);
            if (_path != null) _path.SetActive(false);
            if (_daily != null) _daily.SetActive(false);
            if (_gallery != null) _gallery.SetActive(false);
            if (_streak != null) _streak.SetActive(false);
        }

        public void ShowHome()
        {
            Hide();
            _home.SetActive(true);
            if (_settings != null) _settings.SetActive(false);
            RefreshHome();
        }

        public void ShowPath()
        {
            Hide();
            _path.SetActive(true);
            RefreshPath();
        }

        public void ShowDaily()
        {
            ShowHome();
            _daily.SetActive(true);
            _daily.transform.SetAsLastSibling();
            RefreshDaily();
            Analytics.Track("daily_shown");
        }

        public void ShowGallery()
        {
            Hide();
            _gallery.SetActive(true);
            RefreshGallery();
        }

        public void ShowStreak()
        {
            ShowHome();
            _streak.SetActive(true);
            _streak.transform.SetAsLastSibling();
            RefreshStreak();
        }

        public void ShowSettings(bool on)
        {
            if (_settings == null) return;
            _settings.SetActive(on);
            if (!on) return;
            _settings.transform.SetAsLastSibling();
            SetButton(_settings.transform, "Sound", Copy.Get(SaveService.Data.muted ? "soundOff" : "soundOn"));
            SetButton(_settings.transform, "Theme", Copy.Get(ThemeConfig.IsLight ? "themeToDark" : "themeToLight"));
            SetButton(_settings.transform, "Lang", Copy.Get(SaveService.Data.lang == "tr" ? "langToEn" : "langToTr"));
        }

        static void SetButton(Transform root, string name, string text)
        {
            var t = root.Find(name + "/Label");
            if (t == null) return;
            var label = t.GetComponent<Text>();
            if (label != null) label.text = text;
        }

        void RefreshHome()
        {
            var d = SaveService.Data;
            int next = Mathf.Clamp(d.maxLevel + 1, 1, LevelLoader.Count);
            if (_playSub != null) _playSub.text = Copy.Get("levelWord") + " " + next;
            if (_livesText != null) _livesText.text = Economy.Lives.ToString();
            if (_coinsText != null) _coinsText.text = d.coins.ToString();
            if (_timerText != null)
            {
                var left = Economy.NextLifeIn;
                _timerText.gameObject.SetActive(Economy.Lives < EconomyConfig.MaxLives);
                if (Economy.Lives < EconomyConfig.MaxLives) _timerText.text = left.ToString(@"mm\:ss");
            }
            bool daily = Economy.DailyUnlocked;
            bool streak = Economy.StreakUnlocked;
            if (_dailyLock != null) _dailyLock.text = daily ? Copy.Get("dailyReady") : Copy.Get("opens10");
            if (_dailyBadge != null) _dailyBadge.gameObject.SetActive(daily && !Economy.DailyClaimedToday());
            if (_streakLock != null) _streakLock.text = streak ? d.streak + "/3" : Copy.Get("opens20");
            if (_streakReward != null) _streakReward.text = streak ? Copy.Get("streakReward") : "";
            PlaceHome(Economy.GalleryUnlocked);
            PaintPips(d.streak, streak);
            RebuildHero(d.maxLevel <= 0 ? 0 : d.maxLevel - 1);
            RebuildStrip();
        }

        void PaintPips(int streak, bool on)
        {
            if (_pips == null) return;
            for (int i = 0; i < _pips.Length; i++)
            {
                _pips[i].color = on && i < streak ? ViewKit.Gold : new Color(0.42f, 0.28f, 0.18f, 0.35f);
            }
        }

        void RebuildHero(int levelIndex)
        {
            if (_hero == null) return;
            for (int i = _hero.childCount - 1; i >= 0; i--) Kill(_hero.GetChild(i).gameObject);
            var data = LevelLoader.Get(Mathf.Clamp(levelIndex, 0, LevelLoader.Count - 1));
            RugPortrait.Draw(_hero, data.rows, true, false);
        }

        void RebuildStrip()
        {
            if (_galleryStrip == null) return;
            for (int i = _galleryStrip.childCount - 1; i >= 0; i--) Kill(_galleryStrip.GetChild(i).gameObject);
            bool on = Economy.GalleryUnlocked;
            _galleryStrip.gameObject.SetActive(on);
            if (!on) return;
            var hung = SaveService.Data.hung;
            if (hung == null || hung.Count == 0)
            {
                var note = Txt(_galleryStrip, "Empty", Vector2.zero, Vector2.one, Copy.Get("galleryEmpty"), 26, TextAnchor.MiddleCenter, ThemeConfig.Muted);
                note.raycastTarget = false;
                return;
            }
            int n = Mathf.Min(4, hung.Count);
            // Square slots so every thumb gets the same thin frame and the pattern fills it.
            const float side = 0.20f;
            float gap = 0.03f;
            float start = 0.5f - (n * side + (n - 1) * gap) * 0.5f;
            for (int i = 0; i < n; i++)
            {
                float x0 = start + i * (side + gap);
                var box = new GameObject("Mini" + i, typeof(RectTransform));
                box.transform.SetParent(_galleryStrip, false);
                var rt = box.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(x0, 0.06f);
                rt.anchorMax = new Vector2(x0 + side, 0.94f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                var level = LevelLoader.Get(Mathf.Clamp(hung[hung.Count - 1 - i], 0, LevelLoader.Count - 1));
                RugPortrait.Draw(rt, level.rows, false, true);
            }
        }

        void RefreshPath()
        {
            if (_pathContent == null) return;
            for (int i = _pathContent.childCount - 1; i >= 0; i--) Kill(_pathContent.GetChild(i).gameObject);
            int current = Mathf.Clamp(SaveService.Data.maxLevel, 0, LevelLoader.Count - 1);
            const float step = 168f;
            float height = LevelLoader.Count * step + 220f;
            _pathContent.sizeDelta = new Vector2(0f, height);
            const float yarnW = 22f;
            var yarnColor = ViewKit.Hex("#A56B3C");
            var woolColor = new Color(0.42f, 0.26f, 0.16f, 0.28f);
            for (int i = 0; i < LevelLoader.Count - 1; i++)
            {
                bool doneA = i < SaveService.Data.maxLevel;
                bool nowA = i == current;
                bool doneB = i + 1 < SaveService.Data.maxLevel;
                bool nowB = i + 1 == current;
                float sizeA = nowA ? 156f : doneA ? 118f : 96f;
                float sizeB = nowB ? 156f : doneB ? 118f : 96f;
                float x0 = i % 2 == 0 ? -78f : 78f;
                float x1 = (i + 1) % 2 == 0 ? -78f : 78f;
                float y0 = -110f - i * step;
                float y1 = -110f - (i + 1) * step;
                float leave = y0 - sizeA * 0.42f;
                float enter = y1 + sizeB * 0.42f;
                float mid = (leave + enter) * 0.5f;
                YarnBar(_pathContent, "WoolV" + i + "a", x0 + 3f, (leave + mid) * 0.5f - 2f, yarnW + 10f, Mathf.Abs(mid - leave), woolColor);
                YarnBar(_pathContent, "WoolH" + i, (x0 + x1) * 0.5f + 3f, mid - 2f, Mathf.Abs(x1 - x0) + yarnW, yarnW + 10f, woolColor);
                YarnBar(_pathContent, "WoolV" + i + "b", x1 + 3f, (mid + enter) * 0.5f - 2f, yarnW + 10f, Mathf.Abs(enter - mid), woolColor);
                YarnBar(_pathContent, "YarnV" + i + "a", x0, (leave + mid) * 0.5f, yarnW, Mathf.Abs(mid - leave) + yarnW, yarnColor);
                YarnBar(_pathContent, "YarnH" + i, (x0 + x1) * 0.5f, mid, Mathf.Abs(x1 - x0) + yarnW, yarnW, yarnColor);
                YarnBar(_pathContent, "YarnV" + i + "b", x1, (mid + enter) * 0.5f, yarnW, Mathf.Abs(enter - mid) + yarnW, yarnColor);
            }
            for (int i = 0; i < LevelLoader.Count; i++)
            {
                bool done = i < SaveService.Data.maxLevel;
                bool now = i == current;
                bool locked = i > SaveService.Data.maxLevel;
                float size = now ? 156f : done ? 118f : 96f;
                float nx = i % 2 == 0 ? -78f : 78f;
                float ny = -110f - i * step;
                var shade = Panel(_pathContent, "Sh" + i, Vector2.zero, Vector2.zero, new Color(0.25f, 0.12f, 0.06f, 0.22f));
                var srt = shade.GetComponent<RectTransform>();
                srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 1f);
                srt.pivot = new Vector2(0.5f, 0.5f);
                srt.sizeDelta = new Vector2(size, size);
                srt.anchoredPosition = new Vector2(nx + 5f, ny - 6f);
                shade.GetComponent<Image>().sprite = Glyphs.SolidCircle;
                var node = Panel(_pathContent, "N" + i, Vector2.zero, Vector2.zero, locked ? new Color(0.86f, 0.78f, 0.66f, 1f) : now ? ViewKit.Gold : ViewKit.Hex("#F3E6CF"));
                var rt = node.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(size, size);
                rt.anchoredPosition = new Vector2(nx, ny);
                var img = node.GetComponent<Image>();
                img.sprite = Glyphs.SolidCircle;
                node.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                if (!locked)
                {
                    var b = node.gameObject.AddComponent<Button>();
                    int captured = i;
                    b.onClick.AddListener(() => Fire("lv" + captured));
                }
                string mark = now || locked ? (i + 1).ToString() : "✓";
                int face = i + 1;
                bool marked = face % 10 == 5 || face % 10 == 9;
                var label = Txt(node, "L", marked ? new Vector2(0f, 0.28f) : Vector2.zero, Vector2.one, mark, now ? 42 : 28, TextAnchor.MiddleCenter, locked ? ThemeConfig.Muted : now ? ViewKit.Ink : ThemeConfig.Ink);
                label.raycastTarget = false;
                if (marked)
                {
                    bool super = face % 10 == 9;
                    float pillW = super ? 168f : 108f;
                    float pillH = 44f;
                    float outward = i % 2 == 0 ? -1f : 1f;
                    float px = nx + outward * (size * 0.5f + pillW * 0.5f + 10f);
                    float edge = 520f;
                    px = Mathf.Clamp(px, -edge + pillW * 0.5f, edge - pillW * 0.5f);
                    var pill = Panel(_pathContent, "Pill" + i, Vector2.zero, Vector2.zero, super ? ViewKit.Hex("#8E44AD") : ViewKit.Hex("#C0392B"));
                    var prt = pill.GetComponent<RectTransform>();
                    prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 1f);
                    prt.pivot = new Vector2(0.5f, 0.5f);
                    prt.sizeDelta = new Vector2(pillW, pillH);
                    prt.anchoredPosition = new Vector2(px, ny - size * 0.18f);
                    pill.GetComponent<Image>().sprite = ArtKit.UiRound;
                    pill.GetComponent<Image>().type = Image.Type.Sliced;
                    var tag = Txt(pill, "Tag", Vector2.zero, Vector2.one, super ? Copy.Get("pillSuper") : Copy.Get("pillHard"), 24, TextAnchor.MiddleCenter, Color.white);
                    tag.raycastTarget = false;
                    tag.resizeTextForBestFit = true;
                    tag.resizeTextMinSize = 16;
                    tag.resizeTextMaxSize = 24;
                }
                if (done && !now)
                {
                    var mini = new GameObject("Mini", typeof(RectTransform));
                    mini.transform.SetParent(node, false);
                    var mrt = mini.GetComponent<RectTransform>();
                    mrt.anchorMin = new Vector2(0.08f, 0.08f);
                    mrt.anchorMax = new Vector2(0.92f, 0.92f);
                    mrt.offsetMin = mrt.offsetMax = Vector2.zero;
                    RugPortrait.Draw(mrt, LevelLoader.Get(i).rows, false, true);
                    label.text = "";
                }
            }
            Canvas.ForceUpdateCanvases();
            if (_scroll != null)
            {
                float view = 900f;
                float target = Mathf.Clamp(40f + current * step - view * 0.58f, 0f, Mathf.Max(0f, height - view));
                _pathContent.anchoredPosition = new Vector2(0f, target);
            }
        }

        void RefreshDaily()
        {
            int offer = Economy.DailyOffer();
            int shown = offer < 0 ? (SaveService.Data.dailyDay + 6) % 7 : offer;
            var root = _daily.transform.Find("Card");
            if (root == null) return;
            for (int i = 0; i < 7; i++)
            {
                var cell = root.Find("Day" + i);
                if (cell == null) continue;
                var img = cell.GetComponent<Image>();
                bool today = i == shown;
                bool claimed = offer < 0 ? i <= shown : i < offer;
                if (img != null) img.color = today ? ViewKit.Gold : new Color(0.96f, 0.91f, 0.82f, 1f);
                var day = cell.Find("Day");
                if (day != null) day.GetComponent<Text>().text = Copy.Get("day") + " " + (i + 1);
                var amt = cell.Find("Amt");
                if (amt != null)
                {
                    var at = amt.GetComponent<Text>();
                    at.text = i == 6 ? Copy.Get("dailyPack") : Economy.DailyCoin(i).ToString();
                    at.color = ThemeConfig.Ink;
                }
                var check = cell.Find("Check");
                if (check != null) check.gameObject.SetActive(claimed && !today);
            }
            var claim = root.Find("Claim");
            if (claim != null)
            {
                var t = claim.GetComponentInChildren<Text>();
                if (t != null) t.text = offer < 0 ? Copy.Get("claimed") : Copy.Get("claim");
                var b = claim.GetComponent<Button>();
                if (b != null) b.interactable = offer >= 0;
            }
        }

        void RefreshStreak()
        {
            var d = SaveService.Data;
            PaintPips(d.streak, true);
            if (_modalPips != null)
            {
                for (int i = 0; i < _modalPips.Length; i++)
                    if (_modalPips[i] != null) _modalPips[i].color = i < d.streak ? ViewKit.Gold : new Color(0.82f, 0.74f, 0.62f, 1f);
            }
            if (_streakCount != null) _streakCount.text = d.streak + " / 3";
            if (_streakFoot != null)
                _streakFoot.text = d.streakBroken && d.streak <= 0 ? Copy.Get("streakLost") : Copy.Get("streakReset");
            if (_streakReward != null) _streakReward.text = Copy.Get("streakReward");
        }

        void RefreshGallery()
        {
            var grid = _gallery.transform.Find("Grid");
            if (grid == null) return;
            for (int i = grid.childCount - 1; i >= 0; i--) Kill(grid.GetChild(i).gameObject);
            var hung = SaveService.Data.hung;
            if (hung == null || hung.Count == 0)
            {
                Txt(grid, "Empty", new Vector2(0.08f, 0.4f), new Vector2(0.92f, 0.6f), Copy.Get("galleryEmpty"), 32, TextAnchor.MiddleCenter, ThemeConfig.Muted);
                return;
            }
            for (int i = 0; i < hung.Count; i++)
            {
                int col = i % 3, row = i / 3;
                float x0 = 0.06f + col * 0.31f;
                float y1 = 0.92f - row * 0.28f;
                var box = new GameObject("G" + i, typeof(RectTransform));
                box.transform.SetParent(grid, false);
                var rt = box.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(x0, y1 - 0.24f);
                rt.anchorMax = new Vector2(x0 + 0.28f, y1);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                var level = LevelLoader.Get(Mathf.Clamp(hung[i], 0, LevelLoader.Count - 1));
                RugPortrait.Draw(rt, level.rows, true, true);
            }
        }

        void PlaceHome(bool gallery)
        {
            SetBox(GameObjectOf(_hero), 0.16f, gallery ? 0.56f : 0.50f, 0.84f, 0.82f);
            var shadow = _home.transform.Find("HeroShadow");
            if (shadow != null) SetBox(shadow.gameObject, 0.2f, gallery ? 0.545f : 0.485f, 0.82f, gallery ? 0.60f : 0.54f);
            if (_galleryStrip != null) SetBox(_galleryStrip.gameObject, 0.08f, 0.475f, 0.92f, 0.55f);
            if (_playBtn != null) SetBox(_playBtn.gameObject, 0.1f, gallery ? 0.355f : 0.385f, 0.9f, gallery ? 0.455f : 0.485f);
            float y0 = gallery ? 0.16f : 0.18f;
            float y1 = gallery ? 0.33f : 0.36f;
            PlaceCard("PathCard", 0.05f, y0, 0.33f, y1);
            PlaceCard("DailyCard", 0.36f, y0, 0.64f, y1);
            PlaceCard("StreakCard", 0.67f, y0, 0.95f, y1);
        }

        void PlaceCard(string name, float x0, float y0, float x1, float y1)
        {
            var t = _home.transform.Find(name);
            if (t != null) SetBox(t.gameObject, x0, y0, x1, y1);
            var sh = _home.transform.Find(name + "Sh");
            if (sh != null) SetBox(sh.gameObject, x0 + 0.008f, y0 - 0.008f, x1 + 0.008f, y1 - 0.012f);
        }

        static void SetBox(GameObject go, float x0, float y0, float x1, float y1)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static GameObject GameObjectOf(RectTransform rt) { return rt != null ? rt.gameObject : null; }

        void BuildHome(Transform root)
        {
            var bar = Panel(root, "Top", new Vector2(0.04f, 0.915f), new Vector2(0.96f, 0.975f), new Color(0, 0, 0, 0));
            var row = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.childAlignment = TextAnchor.MiddleLeft;
            row.spacing = 14f;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            row.padding = new RectOffset(0, 0, 6, 6);
            var gear = Btn(bar, "Gear", Vector2.zero, Vector2.one, "⚙", "settings", false);
            var gle = gear.gameObject.AddComponent<LayoutElement>();
            gle.preferredWidth = 84f;
            gle.preferredHeight = 84f;
            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(bar, false);
            var spe = spacer.AddComponent<LayoutElement>();
            spe.flexibleWidth = 1f;
            var lives = Chip(bar, "Lives");
            Txt(lives, "Heart", Vector2.zero, Vector2.one, "♥", 30, TextAnchor.MiddleCenter, new Color(0.86f, 0.28f, 0.34f));
            Preferred(lives.Find("Heart").gameObject, 32f, 44f);
            _livesText = Txt(lives, "N", Vector2.zero, Vector2.one, "5", 30, TextAnchor.MiddleLeft, ViewKit.Cream);
            Preferred(_livesText.gameObject, 28f, 44f);
            _timerText = Txt(lives, "T", Vector2.zero, Vector2.one, "", 16, TextAnchor.MiddleLeft, ViewKit.Gold);
            Preferred(_timerText.gameObject, 58f, 44f);
            _timerText.gameObject.SetActive(false);
            var coins = Chip(bar, "Coins");
            var coinIcon = Panel(coins, "Icon", Vector2.zero, Vector2.one, Color.white);
            var ci = coinIcon.GetComponent<Image>();
            ci.sprite = ArtKit.Coin;
            ci.raycastTarget = false;
            Preferred(coinIcon.gameObject, 36f, 36f);
            _coinsText = Txt(coins, "N", Vector2.zero, Vector2.one, "300", 30, TextAnchor.MiddleLeft, ViewKit.Cream);
            Preferred(_coinsText.gameObject, 72f, 40f);

            Txt(root, "Word", new Vector2(0.08f, 0.855f), new Vector2(0.92f, 0.91f), "Rugtopia", 72, TextAnchor.MiddleCenter, ThemeConfig.Ink);
            Txt(root, "Tag", new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.855f), Copy.Get("tagline"), 30, TextAnchor.MiddleCenter, ViewKit.Gold);

            var shadow = Panel(root, "HeroShadow", new Vector2(0.2f, 0.485f), new Vector2(0.82f, 0.54f), new Color(0.25f, 0.12f, 0.06f, 0.28f));
            shadow.GetComponent<Image>().sprite = ArtKit.UiRound;
            shadow.GetComponent<Image>().type = Image.Type.Sliced;
            _hero = Panel(root, "Hero", new Vector2(0.16f, 0.50f), new Vector2(0.84f, 0.82f), new Color(0, 0, 0, 0)).GetComponent<RectTransform>();
            _galleryStrip = Panel(root, "Strip", new Vector2(0.08f, 0.475f), new Vector2(0.92f, 0.55f), new Color(0, 0, 0, 0)).GetComponent<RectTransform>();

            var play = Btn(root, "Play", new Vector2(0.1f, 0.385f), new Vector2(0.9f, 0.485f), Copy.Get("play"), "play", true);
            _playBtn = play.GetComponent<RectTransform>();
            play.gameObject.AddComponent<PlayBreath>();
            var playLabel = play.transform.Find("Label").GetComponent<RectTransform>();
            playLabel.anchorMin = new Vector2(0.08f, 0.50f);
            playLabel.anchorMax = new Vector2(0.92f, 0.74f);
            _playSub = Txt(play.transform, "Sub", new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.50f), "Level 1", 26, TextAnchor.MiddleCenter, new Color(1f, 0.93f, 0.75f, 1f));
            _playSub.raycastTarget = false;

            BuildHub(root, "PathCard", 0.05f, Copy.Get("path"), "path");
            BuildHub(root, "DailyCard", 0.36f, Copy.Get("daily"), "daily");
            BuildHub(root, "StreakCard", 0.67f, Copy.Get("streak"), "streak");
            _dailyLock = root.Find("DailyCard/Lock").GetComponent<Text>();
            _dailyBadge = root.Find("DailyCard/Badge").GetComponent<Image>();
            _streakLock = root.Find("StreakCard/Lock").GetComponent<Text>();
            _streakReward = root.Find("StreakCard/Reward").GetComponent<Text>();
            _pips = new Image[3];
            for (int i = 0; i < 3; i++) _pips[i] = root.Find("StreakCard/Pip" + i).GetComponent<Image>();
        }

        RectTransform Chip(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = ArtKit.UiRound;
            img.type = Image.Type.Sliced;
            img.color = ThemeConfig.Button;
            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 18, 8, 8);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var fit = go.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return go.GetComponent<RectTransform>();
        }

        static void Preferred(GameObject go, float w, float h)
        {
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = w;
            le.preferredHeight = h;
        }

        void BuildHub(Transform root, string name, float x, string title, string id)
        {
            var shadow = Panel(root, name + "Sh", new Vector2(x + 0.008f, 0.172f), new Vector2(x + 0.298f, 0.348f), new Color(0.25f, 0.12f, 0.06f, 0.22f));
            shadow.GetComponent<Image>().sprite = ArtKit.UiRound;
            shadow.GetComponent<Image>().type = Image.Type.Sliced;
            var card = Btn(root, name, new Vector2(x, 0.18f), new Vector2(x + 0.28f, 0.36f), "", id, false);
            card.GetComponent<Image>().color = ViewKit.Hex("#F6E7CF");
            Txt(card.transform, "Title", new Vector2(0.06f, 0.72f), new Vector2(0.94f, 0.96f), title, 24, TextAnchor.MiddleCenter, ThemeConfig.Ink);
            var lockT = Txt(card.transform, "Lock", new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.32f), "", 16, TextAnchor.MiddleCenter, ThemeConfig.Muted);
            lockT.raycastTarget = false;
            if (name == "PathCard")
            {
                // Card is taller than wide (about 302 x 421). A square dot is ~36px.
                Color[] colors = { ViewKit.Hex("#5C3B28"), ViewKit.Hex("#8A5A3C"), ViewKit.Gold };
                for (int i = 0; i < 3; i++)
                {
                    float py = 0.36f + i * 0.12f;
                    var dot = Panel(card.transform, "Dot" + i, new Vector2(0.44f, py), new Vector2(0.56f, py + 0.085f), colors[i]);
                    var di = dot.GetComponent<Image>();
                    di.sprite = Glyphs.SolidCircle;
                    di.preserveAspect = true;
                }
            }
            if (name == "DailyCard")
            {
                var box = Panel(card.transform, "Gift", new Vector2(0.28f, 0.32f), new Vector2(0.72f, 0.62f), ViewKit.Hex("#E7B15A"));
                var vert = Panel(box, "V", new Vector2(0.38f, 0f), new Vector2(0.62f, 1f), ViewKit.Hex("#C0392B"));
                vert.GetComponent<Image>().raycastTarget = false;
                var horiz = Panel(box, "H", new Vector2(0f, 0.36f), new Vector2(1f, 0.64f), ViewKit.Hex("#C0392B"));
                horiz.GetComponent<Image>().raycastTarget = false;
                var bowL = Panel(card.transform, "BowL", new Vector2(0.28f, 0.56f), new Vector2(0.50f, 0.72f), ViewKit.Hex("#C0392B"));
                var bowR = Panel(card.transform, "BowR", new Vector2(0.50f, 0.56f), new Vector2(0.72f, 0.72f), ViewKit.Hex("#C0392B"));
                bowL.GetComponent<Image>().sprite = Glyphs.SolidCircle;
                bowR.GetComponent<Image>().sprite = Glyphs.SolidCircle;
                bowL.GetComponent<Image>().preserveAspect = true;
                bowR.GetComponent<Image>().preserveAspect = true;
                var knot = Panel(card.transform, "Knot", new Vector2(0.44f, 0.58f), new Vector2(0.56f, 0.70f), ViewKit.Hex("#8E2A22"));
                knot.GetComponent<Image>().sprite = Glyphs.SolidCircle;
                knot.GetComponent<Image>().preserveAspect = true;
                var badge = Panel(card.transform, "Badge", new Vector2(0.86f, 0.88f), new Vector2(0.96f, 0.97f), ViewKit.Hex("#C0392B"));
                badge.GetComponent<Image>().sprite = Glyphs.SolidCircle;
                badge.GetComponent<Image>().preserveAspect = true;
            }
            if (name == "StreakCard")
            {
                for (int i = 0; i < 3; i++)
                {
                    float px = 0.16f + i * 0.26f;
                    var pip = Panel(card.transform, "Pip" + i, new Vector2(px, 0.40f), new Vector2(px + 0.18f, 0.58f), new Color(0.78f, 0.70f, 0.58f, 1f));
                    var pipImg = pip.GetComponent<Image>();
                    pipImg.sprite = Glyphs.SolidCircle;
                    pipImg.preserveAspect = true;
                }
                var reward = Txt(card.transform, "Reward", new Vector2(0.04f, 0.22f), new Vector2(0.96f, 0.36f), "", 15, TextAnchor.MiddleCenter, ThemeConfig.Ink);
                reward.raycastTarget = false;
            }
        }

        void BuildPath(Transform root)
        {
            Txt(root, "Title", new Vector2(0.22f, 0.915f), new Vector2(0.78f, 0.97f), Copy.Get("path"), 48, TextAnchor.MiddleCenter, ThemeConfig.Ink);
            Btn(root, "Back", new Vector2(0.05f, 0.922f), new Vector2(0.13f, 0.962f), "‹", "home", false);
            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(root, false);
            var srt = scrollGo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0f, 0.04f);
            srt.anchorMax = new Vector2(1f, 0.89f);
            srt.offsetMin = srt.offsetMax = Vector2.zero;
            _scroll = scrollGo.GetComponent<ScrollRect>();
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            var vp = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
            vp.transform.SetParent(scrollGo.transform, false);
            Stretch(vp.GetComponent<RectTransform>());
            var vpImg = vp.GetComponent<Image>();
            vpImg.color = new Color(1f, 1f, 1f, 0.01f);
            vpImg.raycastTarget = true;
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(vp.transform, false);
            _pathContent = content.GetComponent<RectTransform>();
            _pathContent.anchorMin = new Vector2(0f, 1f);
            _pathContent.anchorMax = new Vector2(1f, 1f);
            _pathContent.pivot = new Vector2(0.5f, 1f);
            _pathContent.offsetMin = new Vector2(0f, _pathContent.offsetMin.y);
            _pathContent.offsetMax = new Vector2(0f, _pathContent.offsetMax.y);
            _scroll.viewport = vp.GetComponent<RectTransform>();
            _scroll.content = _pathContent;
        }

        void BuildDaily(Transform root)
        {
            var dim = Panel(root, "Dim", Vector2.zero, Vector2.one, new Color(0.1f, 0.05f, 0.03f, 0.55f));
            dim.GetComponent<Image>().raycastTarget = true;
            var card = Card(root, "Card", new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.78f), ViewKit.Gold);
            Txt(card.transform, "Title", new Vector2(0.12f, 0.86f), new Vector2(0.88f, 0.96f), Copy.Get("dailyTitle"), 36, TextAnchor.MiddleCenter, ThemeConfig.Ink);
            var close = Btn(card.transform, "X", new Vector2(0.84f, 0.88f), new Vector2(0.96f, 0.97f), "×", "home", false);
            close.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            close.GetComponentInChildren<Text>().color = ThemeConfig.Ink;
            for (int i = 0; i < 7; i++)
            {
                int col = i % 4;
                int row = i / 4;
                float x0 = 0.05f + col * 0.235f;
                float y1 = row == 0 ? 0.58f : 0.32f;
                if (i == 6) x0 = 0.52f;
                var cell = Panel(card.transform, "Day" + i, new Vector2(x0, y1), new Vector2(x0 + 0.21f, y1 + 0.22f), ViewKit.Hex("#F6E7CF"));
                cell.GetComponent<Image>().sprite = ArtKit.UiRound;
                cell.GetComponent<Image>().type = Image.Type.Sliced;
                var day = Txt(cell, "Day", new Vector2(0.02f, 0.66f), new Vector2(0.98f, 0.98f), Copy.Get("day") + " " + (i + 1), 32, TextAnchor.MiddleCenter, ThemeConfig.Ink);
                day.raycastTarget = false;
                day.horizontalOverflow = HorizontalWrapMode.Overflow;
                day.verticalOverflow = VerticalWrapMode.Overflow;
                if (i < 6)
                {
                    var icon = Panel(cell, "Coin", new Vector2(0.34f, 0.36f), new Vector2(0.66f, 0.58f), Color.white);
                    var ii = icon.GetComponent<Image>();
                    ii.sprite = ArtKit.Coin;
                    ii.preserveAspect = true;
                    ii.raycastTarget = false;
                }
                else
                {
                    Sprite[] icons = { Glyphs.SlotIcon, Glyphs.CombIcon, Glyphs.ShuttleIcon };
                    // The slot and shuttle marks sit in a short band of their sprite, so their boxes are larger to match the comb.
                    float[] scale = { 1.45f, 1f, 1.35f };
                    for (int k = 0; k < 3; k++)
                    {
                        float cx = 0.22f + k * 0.28f;
                        float s = 0.16f * scale[k];
                        var icon = Panel(cell, "B" + k, new Vector2(cx - s * 0.5f, 0.46f - s * 0.55f), new Vector2(cx + s * 0.5f, 0.46f + s * 0.55f), ThemeConfig.Ink);
                        var ii = icon.GetComponent<Image>();
                        ii.sprite = icons[k];
                        ii.preserveAspect = true;
                        ii.raycastTarget = false;
                    }
                }
                var amt = Txt(cell, "Amt", new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.32f), "", 20, TextAnchor.MiddleCenter, ThemeConfig.Ink);
                amt.raycastTarget = false;
                var check = Txt(cell, "Check", new Vector2(0.62f, 0.62f), new Vector2(0.96f, 0.96f), "✓", 22, TextAnchor.MiddleCenter, ViewKit.Hex("#1E7A46"));
                check.gameObject.SetActive(false);
            }
            Btn(card.transform, "Claim", new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.18f), Copy.Get("claim"), "claim", true);
        }

        void BuildGallery(Transform root)
        {
            Txt(root, "Title", new Vector2(0.18f, 0.9f), new Vector2(0.82f, 0.97f), Copy.Get("gallery"), 48, TextAnchor.MiddleCenter, ThemeConfig.Ink);
            Btn(root, "Back", new Vector2(0.05f, 0.91f), new Vector2(0.18f, 0.97f), "‹", "home", false);
            var grid = new GameObject("Grid", typeof(RectTransform));
            grid.transform.SetParent(root, false);
            Stretch(grid.GetComponent<RectTransform>(), 0.02f, 0.04f, 0.98f, 0.88f);
        }

        void BuildStreak(Transform root)
        {
            var dim = Panel(root, "Dim", Vector2.zero, Vector2.one, new Color(0.1f, 0.05f, 0.03f, 0.45f));
            var card = Card(root, "Card", new Vector2(0.08f, 0.3f), new Vector2(0.92f, 0.72f), ViewKit.Gold);
            var x = Btn(card.transform, "X", new Vector2(0.84f, 0.86f), new Vector2(0.96f, 0.96f), "×", "home", false);
            x.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            x.GetComponentInChildren<Text>().color = ThemeConfig.Ink;
            Txt(card.transform, "Title", new Vector2(0.14f, 0.84f), new Vector2(0.86f, 0.96f), Copy.Get("streak"), 36, TextAnchor.MiddleCenter, ThemeConfig.Ink);
            _streakCount = Txt(card.transform, "Count", new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.82f), "0 / 3", 28, TextAnchor.MiddleCenter, ThemeConfig.Muted);
            _modalPips = new Image[3];
            for (int i = 0; i < 2; i++)
            {
                float left = 0.1f + i * 0.22f + 0.16f;
                float right = 0.1f + (i + 1) * 0.22f;
                var yarn = Panel(card.transform, "Yarn" + i, new Vector2(left - 0.01f, 0.508f), new Vector2(right + 0.01f, 0.532f), ViewKit.Hex("#A56B3C"));
                yarn.GetComponent<Image>().raycastTarget = false;
            }
            for (int i = 0; i < 3; i++)
            {
                float x0 = 0.1f + i * 0.22f;
                var pip = Panel(card.transform, "MPip" + i, new Vector2(x0, 0.44f), new Vector2(x0 + 0.15f, 0.60f), new Color(0.78f, 0.70f, 0.58f, 1f));
                var pipImg = pip.GetComponent<Image>();
                pipImg.sprite = Glyphs.SolidCircle;
                pipImg.preserveAspect = true;
                _modalPips[i] = pipImg;
            }
            var prize = Panel(card.transform, "Prize", new Vector2(0.72f, 0.44f), new Vector2(0.94f, 0.6f), ViewKit.Gold);
            prize.GetComponent<Image>().sprite = ArtKit.UiRound;
            prize.GetComponent<Image>().type = Image.Type.Sliced;
            var prizeText = Txt(prize, "T", Vector2.zero, Vector2.one, "+1", 22, TextAnchor.MiddleCenter, ViewKit.Ink);
            prizeText.raycastTarget = false;
            Txt(card.transform, "PrizeLabel", new Vector2(0.66f, 0.32f), new Vector2(0.98f, 0.44f), Copy.Get("streakReward"), 20, TextAnchor.MiddleCenter, ThemeConfig.Ink);
            _streakFoot = Txt(card.transform, "Reset", new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.24f), Copy.Get("streakReset"), 22, TextAnchor.MiddleCenter, ThemeConfig.Muted);
        }

        void BuildSettings(Transform root)
        {
            Txt(root, "Title", new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.96f), Copy.Get("settings"), 40, TextAnchor.MiddleCenter, ViewKit.Cream);
            Btn(root, "Lang", new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.74f), Copy.Get("langToTr"), "lang", true);
            Btn(root, "Theme", new Vector2(0.1f, 0.40f), new Vector2(0.9f, 0.56f), Copy.Get("themeToDark"), "theme", false);
            Btn(root, "Sound", new Vector2(0.1f, 0.22f), new Vector2(0.9f, 0.38f), Copy.Get("sound"), "sound", false);
            Btn(root, "Close", new Vector2(0.22f, 0.05f), new Vector2(0.78f, 0.18f), Copy.Get("close"), "settingsClose", true);
        }

        void YarnBar(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var bar = Panel(parent, name, Vector2.zero, Vector2.zero, color);
            var rt = bar.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(Mathf.Max(1f, w), Mathf.Max(1f, h));
            rt.anchoredPosition = new Vector2(x, y);
            bar.GetComponent<Image>().raycastTarget = false;
        }

        static void Kill(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }

        void Fire(string id) { if (Clicked != null) Clicked(id); }

        GameObject Screen(Transform parent, string name)
        {
            var go = Panel(parent, name, Vector2.zero, Vector2.one, ThemeConfig.BgTop).gameObject;
            go.GetComponent<Image>().raycastTarget = true;
            return go;
        }

        static void Stretch(RectTransform rt, float x0 = 0f, float y0 = 0f, float x1 = 1f, float y1 = 1f)
        {
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        RectTransform Panel(Transform parent, string name, Vector2 a, Vector2 b, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = a; rt.anchorMax = b; rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = c;
            return rt;
        }

        GameObject Card(Transform parent, string name, Vector2 a, Vector2 b, Color edge)
        {
            var rt = Panel(parent, name, a, b, edge);
            var img = rt.GetComponent<Image>();
            img.sprite = ArtKit.UiRound;
            img.type = Image.Type.Sliced;
            var fill = Panel(rt, "Fill", new Vector2(0.015f, 0.02f), new Vector2(0.985f, 0.98f), ThemeConfig.Card);
            var f = fill.GetComponent<Image>();
            f.sprite = ArtKit.UiRound;
            f.type = Image.Type.Sliced;
            f.raycastTarget = false;
            return rt.gameObject;
        }

        Button Btn(Transform parent, string name, Vector2 a, Vector2 b, string label, string id, bool primary)
        {
            var rt = Panel(parent, name, a, b, primary ? ViewKit.Hex("#C4552A") : ThemeConfig.Button);
            var img = rt.GetComponent<Image>();
            img.sprite = ArtKit.UiRound;
            img.type = Image.Type.Sliced;
            var button = rt.gameObject.AddComponent<Button>();
            var text = Txt(rt, "Label", new Vector2(0.06f, 0.42f), new Vector2(0.94f, 0.95f), label, 36, TextAnchor.MiddleCenter, ViewKit.Cream);
            text.raycastTarget = false;
            string captured = id;
            button.onClick.AddListener(() => Fire(captured));
            return button;
        }

        static Text Txt(Transform parent, string name, Vector2 a, Vector2 b, string s, int size, TextAnchor anchor, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = a; rt.anchorMax = b; rt.offsetMin = rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.font = ViewKit.Font;
            t.text = s;
            t.fontSize = size;
            t.fontStyle = FontStyle.Bold;
            t.alignment = anchor;
            t.color = c;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            return t;
        }
    }

    /// <summary>A woven pattern drawn with UI cells, fringe and a resting shadow. Not the play board.</summary>
    public static class RugPortrait
    {
        public static void Draw(RectTransform box, string[] rows, bool fringe, bool square)
        {
            if (rows == null || rows.Length == 0) return;
            int h = rows.Length, w = rows[0].Length;
            if (square)
            {
                DrawSquare(box, rows, fringe, w, h);
                return;
            }
            var frame = New(box, "Frame", new Vector2(0.04f, fringe ? 0.08f : 0.06f), new Vector2(0.96f, fringe ? 0.92f : 0.94f), new Color(0.42f, 0.26f, 0.16f, 1f));
            frame.GetComponent<Image>().sprite = ArtKit.UiRound;
            frame.GetComponent<Image>().type = Image.Type.Sliced;
            float pad = 0.08f;
            for (int r = 0; r < h; r++)
            for (int c = 0; c < w; c++)
            {
                float x0 = pad + (1f - 2f * pad) * c / w;
                float x1 = pad + (1f - 2f * pad) * (c + 1) / w;
                float y1 = (1f - pad) - (1f - 2f * pad) * r / h;
                float y0 = (1f - pad) - (1f - 2f * pad) * (r + 1) / h;
                char ch = c < rows[r].Length ? rows[r][c] : 'C';
                var cell = New(box, "C" + r + "_" + c, new Vector2(x0 + 0.004f, y0 + 0.004f), new Vector2(x1 - 0.004f, y1 - 0.004f), ViewKit.Of(ch));
                cell.GetComponent<Image>().raycastTarget = false;
            }
            for (int r = 1; r < h; r++)
            {
                float y = (1f - pad) - (1f - 2f * pad) * r / h;
                var line = New(box, "Warp" + r, new Vector2(pad, y - 0.004f), new Vector2(1f - pad, y + 0.004f), new Color(0.2f, 0.1f, 0.05f, 0.16f));
                line.GetComponent<Image>().raycastTarget = false;
            }
            for (int r = 0; r < h; r++)
            {
                float y0 = (1f - pad) - (1f - 2f * pad) * (r + 0.35f) / h;
                float y1 = (1f - pad) - (1f - 2f * pad) * (r + 0.42f) / h;
                var dash = New(box, "Thread" + r, new Vector2(pad + 0.02f, y0), new Vector2(1f - pad - 0.02f, y1), new Color(1f, 0.97f, 0.9f, 0.12f));
                dash.GetComponent<Image>().raycastTarget = false;
            }
            if (!fringe) return;
            for (int i = 0; i < 14; i++)
            {
                float x = 0.08f + i * (0.84f / 14f);
                New(box, "Ft" + i, new Vector2(x, 0.93f), new Vector2(x + 0.012f, 0.99f), new Color(0.85f, 0.76f, 0.62f, 1f));
                New(box, "Fb" + i, new Vector2(x, 0.01f), new Vector2(x + 0.012f, 0.07f), new Color(0.85f, 0.76f, 0.62f, 1f));
            }
        }

        static void DrawSquare(RectTransform box, string[] rows, bool fringe, int w, int h)
        {
            float fringeU = fringe ? 0.7f : 0f;
            const float frameU = 0.18f;
            float unitsH = h + fringeU + frameU;
            float unitsW = w + frameU;
            var hostGo = new GameObject("Fit", typeof(RectTransform), typeof(AspectRatioFitter));
            hostGo.transform.SetParent(box, false);
            var host = hostGo.GetComponent<RectTransform>();
            host.anchorMin = host.anchorMax = new Vector2(0.5f, 0.5f);
            host.pivot = new Vector2(0.5f, 0.5f);
            host.anchoredPosition = Vector2.zero;
            var fitter = hostGo.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = unitsW / unitsH;
            float side = (frameU * 0.5f) / unitsW;
            float fringeBand = (fringeU * 0.5f) / unitsH;
            float frameBand = (frameU * 0.5f) / unitsH;
            float cell = 1f / unitsH;
            var frame = New(host, "Frame", new Vector2(side * 0.25f, fringeBand + frameBand * 0.2f), new Vector2(1f - side * 0.25f, 1f - fringeBand - frameBand * 0.2f), new Color(0.42f, 0.26f, 0.16f, 1f));
            frame.GetComponent<Image>().raycastTarget = false;
            float gridTop = 1f - fringeBand - frameBand;
            for (int r = 0; r < h; r++)
            for (int c = 0; c < w; c++)
            {
                float x0 = side + c / unitsW;
                float y1 = gridTop - r * cell;
                float y0 = y1 - cell;
                char ch = c < rows[r].Length ? rows[r][c] : 'C';
                var tile = New(host, "C" + r + "_" + c, new Vector2(x0 + 0.003f, y0 + 0.003f), new Vector2(x0 + 1f / unitsW - 0.003f, y1 - 0.003f), ViewKit.Of(ch));
                tile.GetComponent<Image>().raycastTarget = false;
            }
            if (!fringe) return;
            int n = Mathf.Max(8, w * 3);
            for (int i = 0; i < n; i++)
            {
                float x = side + (1f - 2f * side) * i / n;
                float fw = (1f - 2f * side) / n * 0.35f;
                New(host, "Ft" + i, new Vector2(x, 1f - fringeBand * 0.85f), new Vector2(x + fw, 0.995f), new Color(0.85f, 0.76f, 0.62f, 1f));
                New(host, "Fb" + i, new Vector2(x, 0.005f), new Vector2(x + fw, fringeBand * 0.85f), new Color(0.85f, 0.76f, 0.62f, 1f));
            }
        }

        static RectTransform New(RectTransform parent, string name, Vector2 a, Vector2 b, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = a; rt.anchorMax = b; rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = c;
            img.raycastTarget = false;
            return rt;
        }
    }

    public class PlayBreath : MonoBehaviour
    {
        void Update()
        {
            float s = 1f + 0.012f * Mathf.Sin(Time.time * 1.6f);
            transform.localScale = new Vector3(s, s, 1f);
        }
    }

    public class ThreadShimmer : MonoBehaviour
    {
        void Update()
        {
            var rt = transform as RectTransform;
            if (rt == null) return;
            float k = Mathf.Repeat(Time.time * 0.12f, 1f);
            rt.anchorMin = new Vector2(0.08f + k * 0.74f, 0.1f);
            rt.anchorMax = new Vector2(0.16f + k * 0.74f, 0.9f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
