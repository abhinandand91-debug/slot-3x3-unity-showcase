using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Slot3x3.EditorTools
{
    /// <summary>
    /// Command-line / menu builds. Batch mode:
    /// Unity -batchmode -quit -projectPath . -executeMethod Slot3x3.EditorTools.BuildScript.BuildAndroidApk
    /// Output goes to Builds/Android (ignored by git).
    /// </summary>
    public static class BuildScript
    {
        private const string Scene = "Assets/Scenes/Classic3x3.unity";

        [MenuItem("Slot3x3/Build/Android APK")]
        public static void BuildAndroidApk()
        {
            Build("Builds/Android/ClassicSlots3x3.apk", false);
        }

        [MenuItem("Slot3x3/Build/Android App Bundle (AAB)")]
        public static void BuildAndroidBundle()
        {
            Build("Builds/Android/ClassicSlots3x3.aab", true);
        }

        private static void Build(string path, bool bundle)
        {
            ProjectSetup.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            EditorUserBuildSettings.buildAppBundle = bundle;
            EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { Scene },
                locationPathName = path,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log("Build " + summary.result + ": " + summary.outputPath + " (" + (summary.totalSize / 1024 / 1024) + " MB, " + summary.totalTime + ")");

            if (summary.result != BuildResult.Succeeded && Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }
}
