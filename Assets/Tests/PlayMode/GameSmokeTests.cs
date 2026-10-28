using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Slot3x3.Tests
{
    /// <summary>
    /// Loads the real scene and plays a spin, to prove the game starts and runs without errors.
    /// Set the environment variable SHOWCASE_SHOT to a file path to also save a screenshot (used for the README).
    /// </summary>
    public class GameSmokeTests
    {
        [UnityTest]
        public IEnumerator SceneLoadsAndASpinCompletes()
        {
            GameServices.Save = new MemorySaveStore(); // never touch the player's real save
            GameServices.Random = new SeededRandomSource(7);
            SceneManager.LoadScene("Classic3x3");
            yield return null;
            yield return new WaitForSeconds(1f);

            var machine = UnityEngine.Object.FindAnyObjectByType<SlotMachine>();
            Assert.IsNotNull(machine, "The scene should contain a SlotMachine");
            var wallet = UnityEngine.Object.FindAnyObjectByType<PlayerWallet>();
            int before = wallet.Coins;

            machine.SpinPressed();
            yield return new WaitForSeconds(6f);
            Assert.AreNotEqual(before, wallet.Coins, "The bet should have been taken (or a win paid)");

            // close the daily reward (or any other) window that greets a new player, so the reels are visible
            foreach (PopupWindow popup in UnityEngine.Object.FindObjectsByType<PopupWindow>(FindObjectsSortMode.None))
                if (popup.IsOpen) popup.Close();
            yield return null;

            SaveScreenshotIfAsked();
        }

        private static void SaveScreenshotIfAsked()
        {
            string path = Environment.GetEnvironmentVariable("SHOWCASE_SHOT");
            if (string.IsNullOrEmpty(path)) return;

            const int width = 1600, height = 1200;
            Camera cam = Camera.main;
            var target = new RenderTexture(width, height, 24);
            cam.targetTexture = target;
            cam.Render();
            RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            File.WriteAllBytes(path, image.EncodeToPNG());
            Debug.Log("Screenshot saved: " + path);
        }
    }
}
