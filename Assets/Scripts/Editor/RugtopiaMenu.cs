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
            BuildPlayerAt("Builds/iOS");
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
                var report = BuildPlayerAt(path);
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

        static BuildReport BuildPlayerAt(string path)
        {
            var opts = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = path,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log("Rugtopia iOS build: " + report.summary.result + " → " + path);
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
