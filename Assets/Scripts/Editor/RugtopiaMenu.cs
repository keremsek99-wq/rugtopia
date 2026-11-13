using System;
using System.Linq;
using Rugtopia.Core;
using Rugtopia.Meta;
using Rugtopia.View;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Rugtopia.EditorTools
{
    /// <summary>Editor helpers so the project can be set up without hand-edited YAML. Menu: Rugtopia/…
    /// CI calls <see cref="BuildIOSBatch"/> (GameCI passes -customBuildPath, -bundleId, -marketingVersion, -iosBuildNumber).</summary>
    public static class RugtopiaMenu
    {
        public const string BundleId = "com.rugtopia.game";   // placeholder — CI can override with -bundleId
        const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Rugtopia/1. Apply Project Settings (iOS, portrait)")]
        public static void ApplySettings()
        {
            ApplyPlayerSettings(null, null, null);
        }

        [MenuItem("Rugtopia/2. Create Main Scene")]
        public static void CreateMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            var c = cam.AddComponent<Camera>();
            c.orthographic = true; c.orthographicSize = 10f; c.transform.position = new Vector3(0, 0, -10);
            c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = ViewKit.Background;
            new GameObject("GameController").AddComponent<GameController>();
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("Rugtopia: created " + ScenePath + " and added it to Build Settings.");
        }

        [MenuItem("Rugtopia/3. Run Core Self-Check (30 levels)")]
        public static void SelfCheck()
        {
            int ok = 0, same = 0;
            for (int i = 0; i < LevelLoader.Count; i++)
            {
                var d = LevelLoader.Get(i);
                var r = Solver.Solve(LevelBuilder.Build(d));
                if (r.Ok) ok++;
                if (r.Ok && d.solution != null && r.Path.SequenceEqual(d.solution)) same++;
                else Debug.LogWarning("Level " + (i + 1) + ": solver ok=" + r.Ok + ", path differs from JS export");
            }
            Debug.Log("Rugtopia self-check: " + ok + "/" + LevelLoader.Count + " solvable, " + same + " with the exact JS solver path (JsonUtility + Core inside Unity).");
        }

        [MenuItem("Rugtopia/4. Build iOS Xcode Project")]
        public static void BuildIOS()
        {
            if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.Length == 0) CreateMainScene();
            ApplyPlayerSettings(null, null, null);
            BuildPlayerAt("Builds/iOS", BuildTarget.iOS);
        }

        /// <summary>GameCI WebGL playtest. Prints "Build succeeded!" so the builder accepts the output.</summary>
        public static void BuildWebGLBatch()
        {
            int code = 1;
            try
            {
                ApplyPlayerSettings(null, Arg("marketingVersion"), null);
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.nameFilesAsHashes = false;
                TryCapture();
                if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.Length == 0) CreateMainScene();
                string path = Arg("customBuildPath");
                if (string.IsNullOrEmpty(path)) path = "Builds/WebGL";
                var report = BuildPlayerAt(path, BuildTarget.WebGL);
                code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
                if (code != 0)
                    Debug.LogError("Rugtopia WebGL build failed: " + report.summary.result + " errors=" + report.summary.totalErrors);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                code = 1;
            }
            EditorApplication.Exit(code);
        }

        static void TryCapture()
        {
            try
            {
                const string dir = "Screenshots";
                System.IO.Directory.CreateDirectory(dir);
                var camGo = new GameObject("ShotCam");
                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 10f;
                cam.transform.position = new Vector3(0, 0, -10);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = ViewKit.Hex("#22140F");
                cam.aspect = 9f / 19.5f;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 30f;
                var board = new GameObject("ShotBoard").AddComponent<BoardView>();
                var belt = new GameObject("ShotBelt").AddComponent<BeltView>();
                var lanes = new GameObject("ShotLanes").AddComponent<LaneView>();
                var hud = new GameObject("ShotHUD").AddComponent<HUD>();
                hud.Build();
                hud.Bind(cam);
                var hand = new GameObject("ShotHand").AddComponent<TutorialHand>();
                var bubble = new GameObject("ShotBubble").AddComponent<SpeechBubble>();
                var backdrop = new GameObject("ShotBackdrop").AddComponent<Backdrop>();
                int w = 540, h = 1170;
                var rt = new RenderTexture(w, h, 24);
                cam.targetTexture = rt;
                var d = LevelLoader.Get(0);
                var g = LevelBuilder.Build(d);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "01-playing.png", g, d, w, h, true, false);
                if (d.solution != null && d.solution.Length > 0)
                {
                    int mid = d.solution.Length / 2;
                    for (int i = 0; i < mid; i++) g.Take(d.solution[i]);
                    Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "02-mid.png", g, d, w, h, false, false);
                    for (int i = mid; i < d.solution.Length; i++) g.Take(d.solution[i]);
                    Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "03-win.png", g, d, w, h, false, true);
                }
                var ftue = LevelBuilder.Build(d);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "iter-ftue.png", ftue, d, w, h, true, false);
                var midGame = LevelBuilder.Build(d);
                if (d.solution != null && d.solution.Length > 1) midGame.Take(d.solution[0]);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "iter-playing.png", midGame, d, w, h, false, false);
                var hard = LevelLoader.Get(8);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "iter-hard.png", LevelBuilder.Build(hard), hard, w, h, false, false, "SUPER HARD");
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "iter-fail.png", midGame, d, w, h, false, false, null, true);
                var solved = LevelBuilder.Build(d);
                if (d.solution != null)
                    for (int i = 0; i < d.solution.Length; i++) solved.Take(d.solution[i]);
                ThemeConfig.Current = ThemeConfig.Theme.Light;
                var lightStart = LevelBuilder.Build(d);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "light-playing.png", lightStart, d, w, h, true, false);
                var lightMid = LevelBuilder.Build(d);
                if (d.solution != null)
                    for (int i = 0; i < d.solution.Length / 2; i++) lightMid.Take(d.solution[i]);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "light-mid.png", lightMid, d, w, h, false, false);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "light-win.png", solved, d, w, h, false, true);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "light-fail.png", lightMid, d, w, h, false, false, null, true);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "light-hard.png", LevelBuilder.Build(hard), hard, w, h, false, false, "SUPER HARD");
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "dense-level-1.png", LevelBuilder.Build(LevelLoader.Get(0)), LevelLoader.Get(0), w, h, true, false);
                var lv9 = LevelLoader.Get(8);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "dense-level-9.png", LevelBuilder.Build(lv9), lv9, w, h, false, false, "SUPER HARD");
                var lv15 = LevelLoader.Get(14);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "dense-level-15.png", LevelBuilder.Build(lv15), lv15, w, h, false, false, "HARD");
                var lv21 = LevelLoader.Get(20);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "dense-level-21.png", LevelBuilder.Build(lv21), lv21, w, h, false, false);
                ThemeConfig.Current = ThemeConfig.Theme.Dark;
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "theme-dark-playing.png", ftue, d, w, h, true, false);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "theme-dark-win.png", solved, d, w, h, false, true);
                ThemeConfig.Current = ThemeConfig.Theme.Light;
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "theme-light-playing.png", ftue, d, w, h, true, false);
                Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "theme-light-win.png", solved, d, w, h, false, true);
                ThemeConfig.Current = ThemeConfig.Theme.Light;
                CaptureShell(cam, rt, hud, board, belt, lanes, hand, bubble, backdrop, dir, w, h);
                Debug.Log("Rugtopia screenshots written to " + dir);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Rugtopia screenshots skipped: " + ex.Message);
            }
        }

        static void Shot(Camera cam, RenderTexture rt, BoardView board, BeltView belt, LaneView lanes, HUD hud, TutorialHand hand, SpeechBubble bubble, Backdrop backdrop, string dir, string name, GameState g, LevelData level, int w, int h, bool showHand, bool winCard, string badge = null, bool failCard = false)
        {
            var leftover = UnityEngine.Object.FindObjectsByType<PlayFx>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < leftover.Length; i++)
                if (leftover[i] != null) UnityEngine.Object.DestroyImmediate(leftover[i].gameObject);
            board.SetVisualHp(-1);
            board.Build(g);
            backdrop.Build(board.area.center, new Vector2(board.area.width, board.area.height));
            belt.Refresh(g);
            lanes.suppressEmpty = winCard;
            lanes.Fit(System.Math.Max(1, g.Lanes.Count), level.index >= 3);
            lanes.Refresh(g);
            hud.ShowPlay();
            hud.HidePanels();
            hud.HideBadge();
            hud.ShowLangChoice(false);
            if (!winCard && level.index > 0 && !string.IsNullOrEmpty(level.tag) && level.index < 5) bubble.Show("Tap the spool that matches the glowing stitch!", new Vector2(0f, -2.55f));
            else bubble.Hide();
            hud.SetTop("Level " + (level.index + 1), EconomyConfig.StartCoins, EconomyConfig.MaxLives);
            hud.SetStatus("");
            cam.backgroundColor = ThemeConfig.BgBot;
            hud.SetBooster("slot", true, 3, EconomyConfig.PriceSlot, false);
            hud.SetBooster("comb", true, 2, EconomyConfig.PriceComb, false);
            hud.SetBooster("shuttle", true, 2, EconomyConfig.PriceShuttle, false);
            hud.SetBoosters(level.index >= 3);
            int hint = TutorialHand.LaneFor(g);
            if (showHand && hint >= 0) hand.Show(lanes.FrontLocal(hint));
            else hand.Hide();
            hud.ApplyTheme();
            if (!string.IsNullOrEmpty(badge)) hud.ShowBadge(badge, badge.StartsWith("SUPER HARD") ? ViewKit.Hex("#8E44AD") : ViewKit.Hex("#C0392B"));
            else hud.HideBadge();
            if (failCard) hud.ShowFail("Belt Jammed", g.StitchesLeft, 1f - g.StitchesLeft / (float)System.Math.Max(1, g.Grid.Length), "Free · +1 slot & continue");
            if (winCard)
            {
                hud.ShowLevelComplete(EconomyConfig.Reward(level.tag));
                hud.SetWinPortrait(level.rows);
                var fx = new GameObject("ShotFx").AddComponent<PlayFx>();
                UnityEngine.Random.InitState(7);
                fx.StampConfetti();
            }
            // Overlay canvases are not in Camera.Render. Bind puts the same HUD on this
            // camera, then we render once the layout has settled (the captured end of frame).
            hud.Bind(cam);
            Canvas.ForceUpdateCanvases();
            cam.Render();
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, name), tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        static void CaptureShell(Camera cam, RenderTexture rt, HUD hud, BoardView board, BeltView belt, LaneView lanes, TutorialHand hand, SpeechBubble bubble, Backdrop backdrop, string dir, int w, int h)
        {
            var save = Rugtopia.Meta.SaveService.Data;
            save.pickedLang = true;
            save.muted = true;
            save.coins = 300;
            save.lives = 5;
            save.streak = 0;
            save.dailyLast = "";
            save.dailyDay = 0;
            save.hung = new System.Collections.Generic.List<int>();
            save.maxLevel = 1;
            hud.ShowMenu(save.coins, 2);
            Grab(cam, rt, hud, dir, "shell-home-fresh.png", w, h);
            hud.ShowMap(1, LevelLoader.Count);
            Grab(cam, rt, hud, dir, "shell-path-early.png", w, h);
            save.maxLevel = 11;
            save.hung = new System.Collections.Generic.List<int> { 0, 5, 9 };
            hud.ShowMenu(save.coins, 12);
            Grab(cam, rt, hud, dir, "shell-home-mid.png", w, h);
            save.maxLevel = 20;
            save.streak = 2;
            hud.ShowMenu(save.coins, 21);
            Grab(cam, rt, hud, dir, "shell-home-streak.png", w, h);
            save.maxLevel = 11;
            save.streak = 0;
            hud.ShowMenu(save.coins, 12);
            hud.ShowDaily();
            Grab(cam, rt, hud, dir, "shell-daily.png", w, h);
            save.dailyLast = System.DateTime.UtcNow.ToString("yyyyMMdd");
            save.dailyDay = 2;
            hud.ShowDaily();
            Grab(cam, rt, hud, dir, "shell-daily-claimed.png", w, h);
            save.dailyLast = "";
            save.dailyDay = 0;
            save.maxLevel = 19;
            save.streak = 2;
            hud.ShowStreakSheet();
            Grab(cam, rt, hud, dir, "shell-streak.png", w, h);
            save.streak = 0;
            save.streakBroken = true;
            hud.ShowStreakSheet();
            Grab(cam, rt, hud, dir, "shell-streak-lost.png", w, h);
            save.streakBroken = false;
            save.maxLevel = 11;
            save.streak = 0;
            hud.ShowMap(11, LevelLoader.Count);
            Grab(cam, rt, hud, dir, "shell-path.png", w, h);
            hud.ShowMenu(save.coins, 12);
            save.muted = false;
            save.musicMuted = false;
            hud.ShowSettings(true);
            Grab(cam, rt, hud, dir, "shell-settings.png", w, h);
            hud.ShowSettings(false);
            save.muted = true;
            var solved = LevelBuilder.Build(LevelLoader.Get(0));
            var d0 = LevelLoader.Get(0);
            if (d0.solution != null) for (int i = 0; i < d0.solution.Length; i++) solved.Take(d0.solution[i]);
            Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "shell-win.png", solved, d0, w, h, false, true);
            hud.ShowLangChoice(true);
            Grab(cam, rt, hud, dir, "shell-win-lang.png", w, h);
            Grab(cam, rt, hud, dir, "shell-win-first.png", w, h);
            hud.ShowLangChoice(false);
            var dense = LevelLoader.Get(8);
            var denseWin = LevelBuilder.Build(dense);
            if (dense.solution != null) for (int i = 0; i < dense.solution.Length; i++) denseWin.Take(dense.solution[i]);
            Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "shell-win-dense.png", denseWin, dense, w, h, false, true);
            var denseFail = LevelBuilder.Build(dense);
            if (dense.solution != null)
            {
                int steps = System.Math.Min(8, dense.solution.Length);
                for (int i = 0; i < steps; i++) denseFail.Take(dense.solution[i]);
            }
            Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "shell-fail.png", denseFail, dense, w, h, false, false, null, true);
            var play1 = LevelLoader.Get(0);
            Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "shell-play-hud.png", LevelBuilder.Build(play1), play1, w, h, false, false);
            var lv9 = LevelLoader.Get(8);
            Shot(cam, rt, board, belt, lanes, hud, hand, bubble, backdrop, dir, "shell-play-9.png", LevelBuilder.Build(lv9), lv9, w, h, false, false, "SUPER HARD");
        }

        static void Grab(Camera cam, RenderTexture rt, HUD hud, string dir, string name, int w, int h)
        {
            hud.Bind(cam);
            Canvas.ForceUpdateCanvases();
            cam.Render();
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir, name), tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        /// <summary>Command line / GameCI: -executeMethod Rugtopia.EditorTools.RugtopiaMenu.BuildIOSBatch</summary>
        public static void BuildIOSBatch()
        {
            int code = 1;
            try
            {
                string bundle = Arg("bundleId");
                string marketing = Arg("marketingVersion");
                string buildNumber = Arg("iosBuildNumber");
                ApplyPlayerSettings(bundle, marketing, buildNumber);
                if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.Length == 0) CreateMainScene();
                string path = Arg("customBuildPath");
                if (string.IsNullOrEmpty(path)) path = "Builds/iOS";
                var report = BuildPlayerAt(path, BuildTarget.iOS);
                code = report.summary.result == BuildResult.Succeeded ? 0 : 1;
                if (code != 0)
                    Debug.LogError("Rugtopia iOS build failed: " + report.summary.result + " errors=" + report.summary.totalErrors);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                code = 1;
            }
            EditorApplication.Exit(code);
        }

        static BuildReport BuildPlayerAt(string path, BuildTarget target)
        {
            var opts = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = path,
                target = target,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log("Rugtopia " + target + " build: " + report.summary.result + " → " + path);
            if (report.summary.result == BuildResult.Succeeded)
            {
                // game-ci validateBuild fails the job unless the log contains this exact sentence.
                Console.WriteLine("Build succeeded!");
            }
            return report;
        }

        static void ApplyPlayerSettings(string bundleId, string marketingVersion, string buildNumber)
        {
            if (string.IsNullOrEmpty(bundleId)) bundleId = BundleId;
            PlayerSettings.companyName = "Rugtopia";
            PlayerSettings.productName = "Rugtopia";
            PlayerSettings.bundleVersion = string.IsNullOrEmpty(marketingVersion) ? "0.1.0" : marketingVersion.TrimStart('v');
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, bundleId);
            PlayerSettings.iOS.buildNumber = string.IsNullOrEmpty(buildNumber) ? "1" : buildNumber;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            // Export only. The macOS job switches the Xcode project to manual signing.
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            AssetDatabase.SaveAssets();
            Debug.Log("Rugtopia: project settings applied (bundle id " + bundleId + ", version " + PlayerSettings.bundleVersion + " (" + PlayerSettings.iOS.buildNumber + "), iOS 15+, portrait, IL2CPP).");
        }

        static string Arg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            string flag = "-" + name;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == flag && !args[i + 1].StartsWith("-")) return args[i + 1];
            }
            return null;
        }
    }
}
