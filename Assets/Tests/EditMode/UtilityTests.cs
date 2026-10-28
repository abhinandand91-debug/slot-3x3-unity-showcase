using NUnit.Framework;
using UnityEngine;

namespace Slot3x3.Tests
{
    public class UtilityTests
    {
        [Test]
        public void CoinsAreGroupedWithSpaces()
        {
            Assert.AreEqual("0", NumberFormat.Coins(0));
            Assert.AreEqual("999", NumberFormat.Coins(999));
            Assert.AreEqual("1 000", NumberFormat.Coins(1000));
            Assert.AreEqual("1 250 000", NumberFormat.Coins(1250000));
        }

        [TestCase(Ease.Linear)]
        [TestCase(Ease.InSine)]
        [TestCase(Ease.OutSine)]
        [TestCase(Ease.InCirc)]
        [TestCase(Ease.OutBounce)]
        public void EasingStartsAtZeroAndEndsAtOne(Ease ease)
        {
            Assert.AreEqual(0f, Easing.Evaluate(ease, 0f), 0.0001f);
            Assert.AreEqual(1f, Easing.Evaluate(ease, 1f), 0.0001f);
        }

        private static PlayerWallet CreateWallet()
        {
            var go = new GameObject("wallet");
            go.SetActive(false);
            var wallet = go.AddComponent<PlayerWallet>();
            var so = new UnityEditor.SerializedObject(wallet);
            so.FindProperty("saveData").boolValue = false;
            so.FindProperty("defaultCoins").intValue = 500;
            so.ApplyModifiedPropertiesWithoutUndo();
            PlayerWalletTestHelper.ResetInstance();
            wallet.Initialize();
            return wallet;
        }

        [Test]
        public void WalletCannotGoNegativeAndKnowsAffordableBets()
        {
            PlayerWallet wallet = CreateWallet();
            Assert.AreEqual(500, wallet.Coins);
            Assert.IsTrue(wallet.HasMoneyForBet(500));
            Assert.IsFalse(wallet.HasMoneyForBet(501));
            wallet.AddCoins(-9999);
            Assert.AreEqual(0, wallet.Coins);
            Object.DestroyImmediate(wallet.gameObject);
        }

        [Test]
        public void HundredPercentLevelProgressRaisesTheLevel()
        {
            PlayerWallet wallet = CreateWallet();
            wallet.AddLevelProgress(60f);
            Assert.AreEqual(0, wallet.Level);
            wallet.AddLevelProgress(60f);
            Assert.AreEqual(1, wallet.Level);
            Assert.AreEqual(0f, wallet.LevelProgress, 0.001f);
            Object.DestroyImmediate(wallet.gameObject);
        }
    }

    internal static class PlayerWalletTestHelper
    {
        /// <summary>The wallet is a singleton; clear it so each test gets a fresh one.</summary>
        public static void ResetInstance()
        {
            typeof(PlayerWallet).GetProperty("Instance").SetValue(null, null);
        }
    }
}

namespace Slot3x3.Tests
{
    public class HoldTests
    {
        private static readonly int[] Table = { 1, 2, 4 };

        [TestCase(0, 1)]
        [TestCase(1, 2)]
        [TestCase(2, 4)]
        [TestCase(3, 4)] // more holds than table entries use the last one
        public void HoldingReelsRaisesTheBetMultiplier(int held, int expected)
        {
            Assert.AreEqual(expected, HoldFeature.MultiplierFor(held, Table));
        }

        [Test]
        public void EmptyTableMeansNoMultiplier()
        {
            Assert.AreEqual(1, HoldFeature.MultiplierFor(2, null));
            Assert.AreEqual(1, HoldFeature.MultiplierFor(2, new int[0]));
        }
    }
}
