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
                cam.backgroundColor = ViewKit.Background;
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
                int w = 540, h = 1170;
                var rt = new RenderTexture(w, h, 24);
                cam.targetTexture = rt;
                var d = LevelLoader.Get(0);
                var g = LevelBuilder.Build(d);
                Shot(cam, rt, board, belt, lanes, hud, hand, dir, "01-playing.png", g, d, w, h, true, false);
                if (d.solution != null && d.solution.Length > 0)
                {
                    int mid = d.solution.Length / 2;
                    for (int i = 0; i < mid; i++) g.Take(d.solution[i]);
                    Shot(cam, rt, board, belt, lanes, hud, hand, dir, "02-mid.png", g, d, w, h, false, false);
                    for (int i = mid; i < d.solution.Length; i++) g.Take(d.solution[i]);
                    Shot(cam, rt, board, belt, lanes, hud, hand, dir, "03-win.png", g, d, w, h, false, true);
                }
                Debug.Log("Rugtopia screenshots written to " + dir);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Rugtopia screenshots skipped: " + ex.Message);
            }
        }

        static void Shot(Camera cam, RenderTexture rt, BoardView board, BeltView belt, LaneView lanes, HUD hud, TutorialHand hand, string dir, string name, GameState g, LevelData level, int w, int h, bool showHand, bool winCard)
        {
            board.SetVisualHp(-1);
            board.Build(g);
            belt.Refresh(g);
            lanes.Refresh(g);
            hud.ShowPlay();
            hud.SetCaption(!winCard && level.index == 0 ? "Tap the spool that matches the glowing stitch." : "");
            hud.SetTop("Lv " + (level.index + 1) + " · " + level.nameEn, EconomyConfig.StartCoins, EconomyConfig.MaxLives);
            hud.SetStatus(g.Status == GameStatus.Won ? "Rug woven" : "Weave " + g.StitchesLeft);
            hud.SetBooster("slot", true, 3, EconomyConfig.PriceSlot, false);
            hud.SetBooster("comb", true, 2, EconomyConfig.PriceComb, false);
            hud.SetBooster("shuttle", true, 2, EconomyConfig.PriceShuttle, false);
            if (showHand) hand.Show(lanes.FrontLocal(TutorialHand.LaneFor(g)));
            else hand.Hide();
            if (winCard) hud.ShowLevelComplete(EconomyConfig.Reward(level.tag));
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
