using System.IO;
using UnityEditor;
using UnityEngine;

namespace Slot3x3.EditorTools
{
    /// <summary>
    /// One-click project configuration: product identity, landscape-only mobile settings and the app icon.
    /// Run from "Slot3x3 > Apply Project Settings" (or in batch mode with -executeMethod Slot3x3.EditorTools.ProjectSetup.Apply).
    /// </summary>
    public static class ProjectSetup
    {
        private const string IconPath = "Assets/Art/Icon/AppIcon.png";

        [MenuItem("Slot3x3/Apply Project Settings")]
        public static void Apply()
        {
            PlayerSettings.companyName = "Slot3x3";
            PlayerSettings.productName = "Classic Slots 3x3";
            PlayerSettings.bundleVersion = "1.0.0";

            // landscape only
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            // Android
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.slot3x3.classic");
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.bundleVersionCode = 1;

            // iOS
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS, "com.slot3x3.classic");
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.buildNumber = "1";

            ApplyIcon();
            AssetDatabase.SaveAssets();
            Debug.Log("Project settings applied.");
        }

        /// <summary>Draws the app icon (the lucky 7 on a violet vignette) and assigns it to every platform.</summary>
        private static void ApplyIcon()
        {
            if (!File.Exists(IconPath)) CreateIcon();

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
        }

        private static void CreateIcon()
        {
            const int size = 1024;
            var icon = new Texture2D(size, size, TextureFormat.RGBA32, false);

            // violet vignette background
            Color center = new Color(0.45f, 0.22f, 0.85f), edge = new Color(0.10f, 0.04f, 0.28f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f)) / (size * 0.72f);
                    icon.SetPixel(x, y, Color.Lerp(center, edge, Mathf.Clamp01(d)));
                }

            // the generated 7 symbol on top
            Texture2D seven = SymbolArt.DrawTexture("7");
            float scale = size * 0.78f / seven.width;
            int w = Mathf.RoundToInt(seven.width * scale), h = Mathf.RoundToInt(seven.height * scale);
            int ox = (size - w) / 2, oy = (size - h) / 2;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Color s = seven.GetPixelBilinear(x / (float)w, y / (float)h);
                    Color bg = icon.GetPixel(ox + x, oy + y);
                    icon.SetPixel(ox + x, oy + y, Color.Lerp(bg, new Color(s.r, s.g, s.b, 1f), s.a));
                }
            icon.Apply();

            Directory.CreateDirectory(Path.GetDirectoryName(IconPath));
            File.WriteAllBytes(IconPath, icon.EncodeToPNG());
            Object.DestroyImmediate(icon);
            Object.DestroyImmediate(seven);
            AssetDatabase.ImportAsset(IconPath);
        }
    }
}
