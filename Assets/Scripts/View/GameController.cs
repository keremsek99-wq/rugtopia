using Rugtopia.Core;
using Rugtopia.Meta;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Rugtopia.View
{
    /// <summary>Vertical-slice game loop: loads a level, routes taps to the Core, refreshes views, pays boosters via Economy.
    /// Core resolves weaving instantly; per-stitch animation (prototype: 0.11 s/stitch) is week 2.</summary>
    public class GameController : MonoBehaviour
    {
        public int levelIndex = -1;          // -1 = continue from save
        public bool ignoreLives = true;      // slice/dev default: don't block on lives

        GameState _g;
        BoardView _board; BeltView _belt; LaneView _lanes; HUD _hud;
        float _t0;
        Camera _cam;

        // Works even in an empty scene: if no GameController exists after the first scene loads, create one.
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
            if (_cam == null) { var cg = new GameObject("Main Camera"); cg.tag = "MainCamera"; _cam = cg.AddComponent<Camera>(); }
            _cam.orthographic = true; _cam.orthographicSize = 10f; _cam.transform.position = new Vector3(0, 0, -10);
            _cam.clearFlags = CameraClearFlags.SolidColor; _cam.backgroundColor = ViewKit.Background;
            _board = Child<BoardView>("Board"); _belt = Child<BeltView>("Belt"); _lanes = Child<LaneView>("Lanes"); _hud = Child<HUD>("HUD");
            _hud.Build();
            _hud.Clicked += OnHud;
        }

        T Child<T>(string name) where T : Component
        {
            var t = GetComponentInChildren<T>();
            if (t != null) return t;
            var go = new GameObject(name); go.transform.SetParent(transform, false); return go.AddComponent<T>();
        }

        void Start()
        {
            SaveService.Load();
            StartLevel(levelIndex >= 0 ? levelIndex : Mathf.Min(SaveService.Data.maxLevel, LevelLoader.Count - 1));
        }

        public void StartLevel(int i)
        {
            if (!ignoreLives && !Economy.HasLife) { _hud.SetStatus("Out of lives — next in " + Economy.NextLifeIn.ToString(@"mm\:ss")); return; }
            levelIndex = Mathf.Clamp(i, 0, LevelLoader.Count - 1);
            _g = LevelLoader.NewGame(levelIndex);
            if (SaveService.Data.streak >= EconomyConfig.StreakForBonusSlot && levelIndex >= EconomyConfig.UnlockStreak) _g.AddSlot(); // streak bonus slot
            if (levelIndex == Economy.UnlockLevel(Booster.Slot)) Economy.GrantIntroBooster(Booster.Slot);
            if (levelIndex == Economy.UnlockLevel(Booster.Comb)) Economy.GrantIntroBooster(Booster.Comb);
            if (levelIndex == Economy.UnlockLevel(Booster.Shuttle)) Economy.GrantIntroBooster(Booster.Shuttle);
            _t0 = Time.time;
            _board.Build(_g);
            _hud.HidePanels();
            RefreshAll();
        }

        void Update()
        {
            if (_g == null || _g.Status != GameStatus.Playing) return;
            Vector2 screen;
            if (!PointerDown(out screen)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            Vector2 world = _cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10));
            int lane, depth;
            if (!_lanes.Hit(world, out lane, out depth)) return;
            bool comb = _g.CombArmed;
            if (comb && !Economy.Consume(Booster.Comb)) { _g.ToggleComb(); RefreshAll(); return; }
            if (!_g.Take(lane, depth)) { _hud.SetStatus(_g.FreeSlots <= 0 ? "Belt is full" : ""); return; }
            AfterAction();
        }

        static bool PointerDown(out Vector2 pos)
        {
#if ENABLE_INPUT_SYSTEM
            var p = Pointer.current;
            if (p != null && p.press.wasPressedThisFrame) { pos = p.position.ReadValue(); return true; }
            pos = Vector2.zero; return false;
#else
            if (Input.GetMouseButtonDown(0)) { pos = Input.mousePosition; return true; }
            pos = Vector2.zero; return false;
#endif
        }

        void AfterAction()
        {
            RefreshAll();
            if (_g.Status == GameStatus.Won)
            {
                float secs = Time.time - _t0;
                int reward = Economy.OnWin(levelIndex, _g.Level.tag, secs);
                _hud.ShowWin("Rug woven!\n+" + reward + " coins · " + _g.Taps + " taps · " + secs.ToString("0") + " s");
                RefreshTop();
            }
            else if (_g.Status == GameStatus.Failed)
                _hud.ShowFail(_g.FreeSlots <= 0 ? "Belt is full" : "Out of spools", EconomyConfig.ContinuePrice(_g.Continues));
        }

        void OnHud(string id)
        {
            if (_g == null) return;
            switch (id)
            {
                case "slot":
                    if (_g.Status == GameStatus.Playing && Economy.Consume(Booster.Slot)) { _g.AddSlot(); AfterAction(); } break;
                case "comb":
                    if (_g.Status != GameStatus.Playing) break;
                    if (_g.CombArmed || Economy.CanAfford(Booster.Comb)) _g.ToggleComb(); // paid when the comb tap happens
                    RefreshAll(); break;
                case "shuttle":
                    if (_g.Status == GameStatus.Playing && Economy.CanAfford(Booster.Shuttle) && _g.UseShuttle()) { Economy.Consume(Booster.Shuttle); AfterAction(); } break;
                case "continue":
                    if (Economy.TryContinue(_g)) { _hud.HidePanels(); AfterAction(); } else _hud.SetStatus("Not enough coins"); break;
                case "retry":
                    Economy.OnFailExit(levelIndex); StartLevel(levelIndex); break;
                case "next":
                    StartLevel(levelIndex + 1); break;
                case "prev":
                    StartLevel(levelIndex - 1); break;
            }
        }

        void RefreshTop()
        {
            var d = _g.Level;
            _hud.SetTop("Lv " + (levelIndex + 1) + " · " + d.nameEn + (d.tag == "ftue" || d.tag == "normal" ? "" : " (" + d.tag + ")"), SaveService.Data.coins, Economy.Lives);
        }

        void RefreshAll()
        {
            _board.Refresh(_g); _belt.Refresh(_g); _lanes.Refresh(_g);
            RefreshTop();
            _hud.SetStatus(_g.CombArmed ? "Comb: tap any of the top 4 spools in a lane" : "Weave " + _g.StitchesLeft + " more stitches · belt " + _g.Belt.Count + "/" + _g.BeltSlots);
            _hud.SetBooster("slot", Economy.IsUnlocked(Booster.Slot, levelIndex), Economy.Inventory(Booster.Slot), Economy.Price(Booster.Slot), false);
            _hud.SetBooster("comb", Economy.IsUnlocked(Booster.Comb, levelIndex), Economy.Inventory(Booster.Comb), Economy.Price(Booster.Comb), _g.CombArmed);
            _hud.SetBooster("shuttle", Economy.IsUnlocked(Booster.Shuttle, levelIndex), Economy.Inventory(Booster.Shuttle), Economy.Price(Booster.Shuttle), false);
        }
    }
}
