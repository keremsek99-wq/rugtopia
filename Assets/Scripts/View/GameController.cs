using System.Collections;
using System.Collections.Generic;
using Rugtopia.Core;
using Rugtopia.Meta;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Rugtopia.View
{
    /// <summary>Menu, level map, and the play loop. The core still resolves a tap instantly;
    /// this plays the spool flight and the 0.11 s stitches afterwards.</summary>
    public class GameController : MonoBehaviour
    {
        public int levelIndex = -1;
        public bool ignoreLives = true;

        enum Mode { Menu, Map, Play }

        GameState _g;
        BoardView _board;
        BeltView _belt;
        LaneView _lanes;
        HUD _hud;
        PlayFx _fx;
        PortraitFrame _frame;
        Backdrop _backdrop;
        TutorialHand _hand;
        SpeechBubble _bubble;
        Camera _cam;
        Mode _mode = Mode.Menu;
        bool _busy;
        float _t0;
        int _rewardedFor = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (FindAnyObjectByType<GameController>() != null) return;
            new GameObject("GameController").AddComponent<GameController>();
        }

        void Awake()
        {
            Application.targetFrameRate = 60;
            _cam = Camera.main;
            if (_cam == null)
            {
                var cg = new GameObject("Main Camera");
                cg.tag = "MainCamera";
                _cam = cg.AddComponent<Camera>();
            }
            _cam.orthographic = true;
            _cam.orthographicSize = 10f;
            _cam.transform.position = new Vector3(0, 0, -10);
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = ViewKit.Hex("#22140F");
            if (_cam.GetComponent<AudioListener>() == null) _cam.gameObject.AddComponent<AudioListener>();
            _frame = _cam.GetComponent<PortraitFrame>();
            if (_frame == null) _frame = _cam.gameObject.AddComponent<PortraitFrame>();
            _board = Child<BoardView>("Board");
            _belt = Child<BeltView>("Belt");
            _lanes = Child<LaneView>("Lanes");
            _hud = Child<HUD>("HUD");
            _fx = Child<PlayFx>("Fx");
            _backdrop = Child<Backdrop>("Backdrop");
            _hand = Child<TutorialHand>("Hand");
            _bubble = Child<SpeechBubble>("Bubble");
            _hud.Build();
            _hud.Bind(_cam);
            _hud.Clicked += OnHud;
            _backdrop.Build(_board.area.center, new Vector2(_board.area.width, _board.area.height));
        }

        T Child<T>(string name) where T : Component
        {
            var t = GetComponentInChildren<T>();
            if (t != null) return t;
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }

        void Start()
        {
            SaveService.Load();
            if (levelIndex >= 0) StartLevel(levelIndex);
            else ShowMenu();
        }

        public void ShowMenu()
        {
            StopAllCoroutines();
            _busy = false;
            _fx.Clear();
            if (_hand != null) _hand.Hide();
            if (_bubble != null) _bubble.Hide();
            _mode = Mode.Menu;
            _g = null;
            _hud.ShowMenu(SaveService.Data.coins, SaveService.Data.maxLevel + 1);
        }

        public void ShowMap()
        {
            StopAllCoroutines();
            _busy = false;
            _fx.Clear();
            if (_hand != null) _hand.Hide();
            if (_bubble != null) _bubble.Hide();
            _mode = Mode.Map;
            _g = null;
            _hud.ShowMap(SaveService.Data.maxLevel, LevelLoader.Count);
        }

        public void StartLevel(int i)
        {
            StopAllCoroutines();
            _busy = false;
            _fx.Clear();
            if (!ignoreLives && !Economy.HasLife)
            {
                _hud.ShowPlay();
                _hud.SetStatus("Out of lives — next in " + Economy.NextLifeIn.ToString(@"mm\:ss"));
                return;
            }
            _mode = Mode.Play;
            levelIndex = Mathf.Clamp(i, 0, LevelLoader.Count - 1);
            _rewardedFor = -1;
            _g = LevelLoader.NewGame(levelIndex);
            if (SaveService.Data.streak >= EconomyConfig.StreakForBonusSlot && levelIndex >= EconomyConfig.UnlockStreak) _g.AddSlot();
            string intro = IntroKey();
            if (levelIndex == Economy.UnlockLevel(Booster.Slot)) Economy.GrantIntroBooster(Booster.Slot);
            if (levelIndex == Economy.UnlockLevel(Booster.Comb)) Economy.GrantIntroBooster(Booster.Comb);
            if (levelIndex == Economy.UnlockLevel(Booster.Shuttle)) Economy.GrantIntroBooster(Booster.Shuttle);
            _t0 = Time.time;
            _board.SetVisualHp(-1);
            _board.Build(_g);
            _hud.ShowPlay();
            _hud.HidePanels();
            if (_bubble != null) _bubble.Show(CaptionFor(_g.Level), new Vector2(0f, -4.45f));
            _hud.SetBoosters(levelIndex >= 3);
            RefreshAll();
            PlaceHand();
            if (intro != null) _hud.ShowIntro(Copy.Get(intro));
        }

        string IntroKey()
        {
            var seen = SaveService.Data.seenIntros;
            if (levelIndex == Economy.UnlockLevel(Booster.Slot) && (seen == null || !seen.Contains("Slot"))) return "introSlot";
            if (levelIndex == Economy.UnlockLevel(Booster.Comb) && (seen == null || !seen.Contains("Comb"))) return "introComb";
            if (levelIndex == Economy.UnlockLevel(Booster.Shuttle) && (seen == null || !seen.Contains("Shuttle"))) return "introShuttle";
            return null;
        }

        static string CaptionFor(LevelData level)
        {
            if (level.knots > 0 && level.index == 20) return Copy.Get("ftueKnot");
            if (!string.IsNullOrEmpty(level.covered) && level.index == 11) return Copy.Get("ftueCovered");
            if (level.hidden > 0 && level.index == 5) return Copy.Get("ftueHidden");
            switch (level.index)
            {
                case 0: return Copy.Get("ftueWeft");
                case 1: return Copy.Get("ftueBelt");
                case 2: return Copy.Get("ftueTip");
                case 3: return Copy.Get("ftueNext");
                case 4: return Copy.Get("ftueOrder");
                default: return "";
            }
        }

        void PlaceHand()
        {
            int hint = _g == null ? -1 : TutorialHand.LaneFor(_g);
            if (_hand == null || hint < 0 || levelIndex > 4 || _busy || _g.Status != GameStatus.Playing)
            {
                if (_hand != null) _hand.Hide();
                return;
            }
            _hand.Show(_lanes.FrontLocal(hint));
        }

        void Update()
        {
            PlaceHand();
            if (_mode != Mode.Play || _busy || _g == null || _g.Status != GameStatus.Playing) return;
            Vector2 screen;
            if (!PointerDown(out screen)) return;
            if (OverUi()) return;
            Vector2 world = _cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10));
            int lane, depth;
            if (!_lanes.Hit(world, out lane, out depth)) return;
            Vector2 from = _lanes.FrontLocal(lane);
            bool comb = _g.CombArmed;
            if (comb && !Economy.Consume(Booster.Comb)) { _g.ToggleComb(); RefreshAll(); return; }
            int hpBefore = _g.Hp;
            if (!_g.Take(lane, depth))
            {
                Feel.Play("no");
                _hud.SetStatus(_g.FreeSlots <= 0 ? "Belt is full" : "");
                return;
            }
            Feel.Tap();
            StartCoroutine(Animate(CopyEvents(), from, hpBefore));
        }

        static bool PointerDown(out Vector2 pos)
        {
#if ENABLE_INPUT_SYSTEM
            var pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame)
            {
                pos = pointer.position.ReadValue();
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                pos = Input.GetTouch(0).position;
                return true;
            }
            if (Input.GetMouseButtonDown(0)) { pos = Input.mousePosition; return true; }
#endif
            pos = Vector2.zero;
            return false;
        }

        static bool OverUi()
        {
            if (EventSystem.current == null) return false;
            return EventSystem.current.IsPointerOverGameObject();
        }

        List<GameEvent> CopyEvents()
        {
            var ev = new List<GameEvent>(_g.Events.Count);
            for (int i = 0; i < _g.Events.Count; i++) ev.Add(_g.Events[i]);
            return ev;
        }

        IEnumerator Animate(List<GameEvent> ev, Vector2 from, int hpBefore)
        {
            _busy = true;
            _board.SetVisualHp(hpBefore);
            _board.Refresh(_g);
            _lanes.Refresh(_g);
            Vector2 slot = _belt.SlotLocal(Mathf.Max(0, _g.Belt.Count - 1), _g.BeltSlots);
            bool flew = false;
            for (int i = 0; i < ev.Count; i++)
            {
                var e = ev[i];
                if (e.Kind == EventKind.SpoolTaken && !flew)
                {
                    flew = true;
                    Feel.Play("tap");
                    Color c = SpoolColor(e.SpoolId);
                    yield return _fx.Fly(from, slot, c, 0.26f);
                    _belt.Refresh(_g);
                }
                else if (e.Kind == EventKind.StitchWoven)
                {
                    _board.SetVisualHp(e.PathIndex + 1);
                    _board.Refresh(_g);
                    var cell = _board.CellPos(e.Row, e.Col);
                    _fx.Thread(slot, cell, ViewKit.Of(_g.Grid.Color[e.Row, e.Col]));
                    Feel.Play("stitch");
                    yield return new WaitForSeconds(0.11f);
                }
                else if (e.Kind == EventKind.RowCompleted)
                {
                    _board.PopRow(e.Row);
                    Feel.Play("row");
                    Feel.Tap();
                    yield return new WaitForSeconds(0.12f);
                }
            }
            _board.SetVisualHp(-1);
            RefreshAll();
            if (_g.Status == GameStatus.Won && _rewardedFor != levelIndex)
            {
                _rewardedFor = levelIndex;
                if (_hand != null) _hand.Hide();
                if (_bubble != null) _bubble.Hide();
                yield return new WaitForSeconds(0.25f);
                float secs = Time.time - _t0;
                int reward = Economy.OnWin(levelIndex, _g.Level.tag, secs);
                Feel.Play("win");
                _fx.Confetti();
                int shown = 0;
                _hud.ShowLevelComplete(0);
                while (shown < reward)
                {
                    shown = Mathf.Min(reward, shown + Mathf.Max(1, reward / 12));
                    _hud.ShowLevelComplete(shown);
                    yield return new WaitForSeconds(0.04f);
                }
                RefreshTop();
            }
            else if (_g.Status == GameStatus.Failed)
            {
                Feel.Play("fail");
                if (_bubble != null) _bubble.Hide();
                _hud.ShowFail(
                    Copy.Get(_g.FreeSlots <= 0 ? "failTitle" : "failSpools"),
                    Copy.Get("failSub"),
                    EconomyConfig.ContinuePrice(_g.Continues));
            }
            _busy = false;
        }

        Color SpoolColor(int id)
        {
            for (int i = 0; i < _g.Belt.Count; i++) if (_g.Belt[i].Id == id) return ViewKit.Of(_g.Belt[i].Color);
            for (int i = 0; i < _g.Lanes.Count; i++)
                for (int j = 0; j < _g.Lanes[i].Count; j++)
                    if (_g.Lanes[i][j].Id == id) return ViewKit.Of(_g.Lanes[i][j].Color);
            return ViewKit.Wool;
        }

        void OnHud(string id)
        {
            if (id == "lang")
            {
                SaveService.Data.lang = SaveService.Data.lang == "tr" ? "en" : "tr";
                SaveService.Save();
                _hud.ApplyLanguage();
                if (_mode == Mode.Menu) ShowMenu();
                else if (_mode == Mode.Map) ShowMap();
                else if (_g != null)
                {
                    RefreshAll();
                    if (_bubble != null) _bubble.Show(CaptionFor(_g.Level), new Vector2(0f, -4.45f));
                }
                return;
            }
            if (id == "introOk") { _hud.HideIntro(); return; }
            if (id == "play" || id == "map") { ShowMap(); return; }
            if (id == "home") { ShowMenu(); return; }
            if (id.Length > 2 && id.StartsWith("lv") && int.TryParse(id.Substring(2), out int li))
            {
                if (li <= SaveService.Data.maxLevel) StartLevel(li);
                return;
            }
            if (_g == null || _busy) return;
            switch (id)
            {
                case "slot":
                    if (_g.Status == GameStatus.Playing && Economy.Consume(Booster.Slot))
                        Commit(() => _g.AddSlot(), new Vector2(0f, _belt.y));
                    break;
                case "comb":
                    if (_g.Status != GameStatus.Playing) break;
                    if (_g.CombArmed || Economy.CanAfford(Booster.Comb)) _g.ToggleComb();
                    RefreshAll();
                    break;
                case "shuttle":
                    if (_g.Status == GameStatus.Playing && Economy.CanAfford(Booster.Shuttle))
                    {
                        int hpBefore = _g.Hp;
                        if (_g.UseShuttle())
                        {
                            Economy.Consume(Booster.Shuttle);
                            StartCoroutine(Animate(CopyEvents(), new Vector2(0f, _belt.y), hpBefore));
                        }
                    }
                    break;
                case "continue":
                    {
                        if (_g.Status != GameStatus.Failed) break;
                        int hpBefore = _g.Hp;
                        if (Economy.TryContinue(_g))
                        {
                            _hud.HidePanels();
                            StartCoroutine(Animate(CopyEvents(), new Vector2(0f, _belt.y), hpBefore));
                        }
                        else _hud.SetStatus("Not enough coins");
                        break;
                    }
                case "retry":
                    Economy.OnFailExit(levelIndex);
                    StartLevel(levelIndex);
                    break;
                case "next":
                    StartLevel(levelIndex + 1);
                    break;
                case "prev":
                    StartLevel(levelIndex - 1);
                    break;
            }
        }

        IEnumerator PunchCamera()
        {
            const float baseSize = 10f;
            float t = 0f;
            while (t < 0.28f)
            {
                t += Time.deltaTime;
                float k = Mathf.Sin(Mathf.Clamp01(t / 0.28f) * Mathf.PI);
                if (_cam != null) _cam.orthographicSize = baseSize - 0.28f * k;
                yield return null;
            }
            if (_cam != null) _cam.orthographicSize = baseSize;
        }

        void Commit(System.Action mutate, Vector2 from)
        {
            int hpBefore = _g.Hp;
            mutate();
            StartCoroutine(Animate(CopyEvents(), from, hpBefore));
        }

        void RefreshTop()
        {
            var d = _g.Level;
            string name = SaveService.Data.lang == "tr" ? d.nameTr : d.nameEn;
            _hud.SetTop("Lv " + (levelIndex + 1) + " · " + name + (d.tag == "ftue" || d.tag == "normal" ? "" : " (" + d.tag + ")"), SaveService.Data.coins, Economy.Lives);
        }

        void RefreshAll()
        {
            _board.Refresh(_g);
            _belt.Refresh(_g);
            _lanes.Refresh(_g);
            RefreshTop();
            _hud.SetStatus(_g.CombArmed ? Copy.Get("combMode") : Copy.Get("weave") + " " + _g.StitchesLeft);
            _hud.SetBooster("slot", Economy.IsUnlocked(Booster.Slot, levelIndex), Economy.Inventory(Booster.Slot), Economy.Price(Booster.Slot), false);
            _hud.SetBooster("comb", Economy.IsUnlocked(Booster.Comb, levelIndex), Economy.Inventory(Booster.Comb), Economy.Price(Booster.Comb), _g.CombArmed);
            _hud.SetBooster("shuttle", Economy.IsUnlocked(Booster.Shuttle, levelIndex), Economy.Inventory(Booster.Shuttle), Economy.Price(Booster.Shuttle), false);
        }
    }
}
