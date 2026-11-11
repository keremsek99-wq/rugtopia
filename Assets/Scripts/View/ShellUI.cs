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
            if (_streakLock != null) _streakLock.text = streak ? (d.streak + "/3") : Copy.Get("opens20");
            if (_streakReward != null) _streakReward.text = Copy.Get("streakReward");
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
            RugPortrait.Draw(_hero, data.rows, true);
            var shine = Panel(_hero, "Shine", new Vector2(0.3f, 0.1f), new Vector2(0.4f, 0.9f), new Color(1f, 0.97f, 0.9f, 0.22f));
            shine.gameObject.AddComponent<ThreadShimmer>();
            shine.SetAsLastSibling();
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
            for (int i = 0; i < n; i++)
            {
                float x0 = 0.04f + i * 0.24f;
                var box = new GameObject("Mini" + i, typeof(RectTransform));
                box.transform.SetParent(_galleryStrip, false);
                var rt = box.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(x0, 0.08f);
                rt.anchorMax = new Vector2(x0 + 0.22f, 0.92f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                var level = LevelLoader.Get(Mathf.Clamp(hung[hung.Count - 1 - i], 0, LevelLoader.Count - 1));
                RugPortrait.Draw(rt, level.rows, false);
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
            for (int i = 0; i < LevelLoader.Count - 1; i++)
            {
                var yarn = Panel(_pathContent, "Yarn" + i, Vector2.zero, Vector2.zero, new Color(0.55f, 0.38f, 0.24f, 0.85f));
                var yrt = yarn.GetComponent<RectTransform>();
                yrt.anchorMin = yrt.anchorMax = new Vector2(0.5f, 1f);
                yrt.pivot = new Vector2(0.5f, 1f);
                yrt.sizeDelta = new Vector2(8f, step - 20f);
                yrt.anchoredPosition = new Vector2(i % 2 == 0 ? -36f : 36f, -90f - i * step);
            }
            for (int i = 0; i < LevelLoader.Count; i++)
            {
                bool done = i < SaveService.Data.maxLevel;
                bool now = i == current;
                bool locked = i > SaveService.Data.maxLevel;
                float size = now ? 132f : 96f;
                var node = Panel(_pathContent, "N" + i, Vector2.zero, Vector2.zero, locked ? new Color(0.72f, 0.62f, 0.5f, 1f) : now ? ViewKit.Gold : ThemeConfig.Button);
                var rt = node.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(size, size);
                rt.anchoredPosition = new Vector2(i % 2 == 0 ? -70f : 70f, -90f - i * step);
                var img = node.GetComponent<Image>();
                img.sprite = Glyphs.Circle;
                if (!locked)
                {
                    var b = node.gameObject.AddComponent<Button>();
                    int captured = i;
                    b.onClick.AddListener(() => Fire("lv" + captured));
                }
                string mark = now || locked ? (i + 1).ToString() : "✓";
                var label = Txt(node, "L", Vector2.zero, Vector2.one, mark, now ? 40 : 28, TextAnchor.MiddleCenter, locked ? ThemeConfig.Muted : ViewKit.Cream);
                label.raycastTarget = false;
                int face = i + 1;
                if (face % 10 == 5 || face % 10 == 9)
                {
                    var tag = Txt(node, "Tag", new Vector2(0.1f, -0.45f), new Vector2(0.9f, -0.05f), face % 10 == 9 ? "SUPER" : "HARD", 16, TextAnchor.MiddleCenter, face % 10 == 9 ? ViewKit.Hex("#8E44AD") : ViewKit.Hex("#C0392B"));
                    tag.raycastTarget = false;
                }
                if (done && !now)
                {
                    var mini = new GameObject("Mini", typeof(RectTransform));
                    mini.transform.SetParent(node, false);
                    var mrt = mini.GetComponent<RectTransform>();
                    mrt.anchorMin = new Vector2(0.18f, 0.18f);
                    mrt.anchorMax = new Vector2(0.82f, 0.62f);
                    mrt.offsetMin = mrt.offsetMax = Vector2.zero;
                    RugPortrait.Draw(mrt, LevelLoader.Get(i).rows, false);
                    label.text = "";
                }
            }
            Canvas.ForceUpdateCanvases();
            if (_scroll != null)
            {
                float view = 900f;
                float target = Mathf.Clamp(90f + current * step - view * 0.45f, 0f, Mathf.Max(0f, height - view));
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
                bool past = offer >= 0 ? i < offer : i <= shown;
                if (img != null) img.color = today ? ViewKit.Gold : past ? ThemeConfig.Button : new Color(0.86f, 0.76f, 0.64f, 1f);
                var label = cell.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = i == 6 ? Copy.Get("dailyPack") : "+" + Economy.DailyCoin(i);
                    label.color = today || past ? ViewKit.Cream : ThemeConfig.Ink;
                }
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
                RugPortrait.Draw(rt, level.rows, true);
            }
        }

        void BuildHome(Transform root)
        {
            var top = Panel(root, "Top", new Vector2(0.04f, 0.91f), new Vector2(0.96f, 0.975f), new Color(0, 0, 0, 0));
            Btn(top, "Gear", new Vector2(0f, 0.05f), new Vector2(0.16f, 0.95f), "⚙", "settings", false);
            var lives = Panel(top, "Lives", new Vector2(0.22f, 0.08f), new Vector2(0.58f, 0.92f), ThemeConfig.Button);
            lives.GetComponent<Image>().sprite = ArtKit.UiRound;
            lives.GetComponent<Image>().type = Image.Type.Sliced;
            Txt(lives, "Heart", new Vector2(0.06f, 0.1f), new Vector2(0.28f, 0.9f), "♥", 32, TextAnchor.MiddleCenter, new Color(0.86f, 0.28f, 0.34f));
            _livesText = Txt(lives, "N", new Vector2(0.28f, 0.35f), new Vector2(0.55f, 0.95f), "5", 32, TextAnchor.MiddleLeft, ViewKit.Cream);
            _timerText = Txt(lives, "T", new Vector2(0.5f, 0.1f), new Vector2(0.96f, 0.9f), "", 20, TextAnchor.MiddleRight, ViewKit.Gold);
            var coins = Panel(top, "Coins", new Vector2(0.62f, 0.08f), new Vector2(1f, 0.92f), ThemeConfig.Button);
            coins.GetComponent<Image>().sprite = ArtKit.UiRound;
            coins.GetComponent<Image>().type = Image.Type.Sliced;
            var coinIcon = Panel(coins, "Icon", new Vector2(0.06f, 0.18f), new Vector2(0.28f, 0.82f), Color.white);
            var ci = coinIcon.GetComponent<Image>();
            ci.sprite = ArtKit.Coin;
            ci.raycastTarget = false;
            _coinsText = Txt(coins, "N", new Vector2(0.32f, 0f), new Vector2(0.96f, 1f), "300", 30, TextAnchor.MiddleLeft, ViewKit.Cream);

            Txt(root, "Word", new Vector2(0.08f, 0.845f), new Vector2(0.92f, 0.905f), "Rugtopia", 78, TextAnchor.MiddleCenter, ThemeConfig.Ink);
            Txt(root, "Tag", new Vector2(0.1f, 0.805f), new Vector2(0.9f, 0.848f), Copy.Get("tagline"), 32, TextAnchor.MiddleCenter, ViewKit.Gold);

            var shadow = Panel(root, "HeroShadow", new Vector2(0.16f, 0.50f), new Vector2(0.88f, 0.56f), new Color(0.25f, 0.12f, 0.06f, 0.28f));
            shadow.GetComponent<Image>().sprite = ArtKit.UiRound;
            shadow.GetComponent<Image>().type = Image.Type.Sliced;
            _hero = Panel(root, "Hero", new Vector2(0.14f, 0.52f), new Vector2(0.86f, 0.80f), new Color(0, 0, 0, 0)).GetComponent<RectTransform>();

            _galleryStrip = Panel(root, "Strip", new Vector2(0.08f, 0.43f), new Vector2(0.92f, 0.51f), new Color(0, 0, 0, 0)).GetComponent<RectTransform>();

            var play = Btn(root, "Play", new Vector2(0.1f, 0.31f), new Vector2(0.9f, 0.41f), Copy.Get("play"), "play", true);
            _playBtn = play.GetComponent<RectTransform>();
            play.gameObject.AddComponent<PlayBreath>();
            _playSub = Txt(play.transform, "Sub", new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.42f), "Level 1", 28, TextAnchor.MiddleCenter, ViewKit.Gold);
            _playSub.raycastTarget = false;

            BuildHub(root, "PathCard", 0.06f, Copy.Get("path"), "path");
            BuildHub(root, "DailyCard", 0.355f, Copy.Get("daily"), "daily");
            BuildHub(root, "StreakCard", 0.65f, Copy.Get("streak"), "streak");
            _dailyLock = root.Find("DailyCard/Lock").GetComponent<Text>();
            _dailyBadge = root.Find("DailyCard/Badge").GetComponent<Image>();
            _streakLock = root.Find("StreakCard/Lock").GetComponent<Text>();
            _streakReward = root.Find("StreakCard/Reward").GetComponent<Text>();
            _pips = new Image[3];
            for (int i = 0; i < 3; i++) _pips[i] = root.Find("StreakCard/Pip" + i).GetComponent<Image>();
        }

        void BuildHub(Transform root, string name, float x, string title, string id)
        {
            var card = Btn(root, name, new Vector2(x, 0.14f), new Vector2(x + 0.29f, 0.29f), "", id, false);
            Txt(card.transform, "Title", new Vector2(0.06f, 0.62f), new Vector2(0.94f, 0.94f), title, 26, TextAnchor.MiddleCenter, ViewKit.Cream);
            var lockT = Txt(card.transform, "Lock", new Vector2(0.06f, 0.28f), new Vector2(0.94f, 0.6f), "", 18, TextAnchor.MiddleCenter, ViewKit.Gold);
            lockT.raycastTarget = false;
            if (name == "DailyCard")
            {
                var badge = Panel(card.transform, "Badge", new Vector2(0.72f, 0.72f), new Vector2(0.96f, 0.96f), ViewKit.Hex("#C0392B"));
                badge.GetComponent<Image>().sprite = Glyphs.Circle;
            }
            if (name == "StreakCard")
            {
                for (int i = 0; i < 3; i++)
                {
                    float px = 0.18f + i * 0.24f;
                    var pip = Panel(card.transform, "Pip" + i, new Vector2(px, 0.12f), new Vector2(px + 0.16f, 0.32f), new Color(1, 1, 1, 0.3f));
                    pip.GetComponent<Image>().sprite = Glyphs.Circle;
                }
                var reward = Txt(card.transform, "Reward", new Vector2(0.04f, 0.34f), new Vector2(0.96f, 0.58f), "", 16, TextAnchor.MiddleCenter, ViewKit.Cream);
                reward.raycastTarget = false;
            }
        }

        void BuildPath(Transform root)
        {
            Txt(root, "Title", new Vector2(0.2f, 0.9f), new Vector2(0.8f, 0.97f), Copy.Get("path"), 52, TextAnchor.MiddleCenter, ThemeConfig.Ink);
            Btn(root, "Back", new Vector2(0.05f, 0.91f), new Vector2(0.2f, 0.97f), "‹", "home", false);
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
            var vp = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            vp.transform.SetParent(scrollGo.transform, false);
            Stretch(vp.GetComponent<RectTransform>());
            var vpImg = vp.GetComponent<Image>();
            vpImg.color = new Color(1f, 1f, 1f, 0.01f);
            vp.GetComponent<Mask>().showMaskGraphic = false;
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
            Txt(card.transform, "Title", new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.96f), Copy.Get("dailyTitle"), 40, TextAnchor.MiddleCenter, ThemeConfig.Ink);
            for (int i = 0; i < 7; i++)
            {
                int col = i % 4, row = i / 4;
                float x0 = 0.06f + col * 0.23f;
                float y1 = 0.72f - row * 0.28f;
                if (i == 6) { x0 = 0.3f; y1 = 0.18f; }
                var cell = Panel(card.transform, "Day" + i, new Vector2(x0, y1), new Vector2(x0 + 0.2f, y1 + 0.22f), ThemeConfig.Button);
                cell.GetComponent<Image>().sprite = ArtKit.UiRound;
                cell.GetComponent<Image>().type = Image.Type.Sliced;
                var label = Txt(cell, "V", Vector2.zero, Vector2.one, (i + 1).ToString(), 22, TextAnchor.MiddleCenter, ViewKit.Cream);
                label.raycastTarget = false;
            }
            Btn(card.transform, "Claim", new Vector2(0.12f, 0.04f), new Vector2(0.88f, 0.16f), Copy.Get("claim"), "claim", true);
            Btn(root, "Close", new Vector2(0.35f, 0.14f), new Vector2(0.65f, 0.2f), Copy.Get("close"), "home", false);
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
            var card = Card(root, "Card", new Vector2(0.1f, 0.34f), new Vector2(0.9f, 0.66f), ViewKit.Gold);
            Txt(card.transform, "Title", new Vector2(0.08f, 0.7f), new Vector2(0.92f, 0.92f), Copy.Get("streak"), 40, TextAnchor.MiddleCenter, ThemeConfig.Ink);
            Txt(card.transform, "Body", new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.68f), Copy.Get("streakBody"), 28, TextAnchor.MiddleCenter, ThemeConfig.Muted);
            Btn(card.transform, "Ok", new Vector2(0.18f, 0.08f), new Vector2(0.82f, 0.28f), Copy.Get("close"), "home", true);
        }

        void BuildSettings(Transform root)
        {
            Txt(root, "Title", new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.96f), Copy.Get("settings"), 40, TextAnchor.MiddleCenter, ViewKit.Cream);
            Btn(root, "Lang", new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.74f), Copy.Get("langToTr"), "lang", true);
            Btn(root, "Theme", new Vector2(0.1f, 0.40f), new Vector2(0.9f, 0.56f), Copy.Get("themeToDark"), "theme", false);
            Btn(root, "Sound", new Vector2(0.1f, 0.22f), new Vector2(0.9f, 0.38f), Copy.Get("sound"), "sound", false);
            Btn(root, "Close", new Vector2(0.22f, 0.05f), new Vector2(0.78f, 0.18f), Copy.Get("close"), "settingsClose", true);
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
        public static void Draw(RectTransform box, string[] rows, bool fringe)
        {
            if (rows == null || rows.Length == 0) return;
            int h = rows.Length, w = rows[0].Length;
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
            if (!fringe) return;
            for (int i = 0; i < 14; i++)
            {
                float x = 0.08f + i * (0.84f / 14f);
                New(box, "Ft" + i, new Vector2(x, 0.93f), new Vector2(x + 0.012f, 0.99f), new Color(0.85f, 0.76f, 0.62f, 1f));
                New(box, "Fb" + i, new Vector2(x, 0.01f), new Vector2(x + 0.012f, 0.07f), new Color(0.85f, 0.76f, 0.62f, 1f));
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
