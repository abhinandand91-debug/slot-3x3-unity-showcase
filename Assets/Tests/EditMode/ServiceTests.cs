using NUnit.Framework;

namespace Slot3x3.Tests
{
    /// <summary>Checks the small services and payout rules that the game code depends on.</summary>
    public class ServiceTests
    {
        [Test]
        public void MemorySaveStoreReturnsWhatWasSaved()
        {
            var store = new MemorySaveStore();
            store.SetInt("coins", 42);
            store.SetString("name", "player");
            Assert.AreEqual(42, store.GetInt("coins", 0));
            Assert.AreEqual("player", store.GetString("name", ""));
        }

        [Test]
        public void MemorySaveStoreUsesTheFallbackForMissingOrDeletedKeys()
        {
            var store = new MemorySaveStore();
            store.SetFloat("progress", 0.5f);
            store.Delete("progress");
            Assert.AreEqual(1f, store.GetFloat("progress", 1f));
            Assert.AreEqual(7, store.GetInt("never-saved", 7));
        }

        [Test]
        public void SeededRandomRepeatsTheSameSequence()
        {
            var a = new SeededRandomSource(123);
            var b = new SeededRandomSource(123);
            for (int i = 0; i < 20; i++) Assert.AreEqual(a.Range(0, 100), b.Range(0, 100));
        }

        [Test]
        public void PayoutScalesByLineBetOnlyWhenAsked()
        {
            Assert.AreEqual(60, Payout.Coins(30, 2, 5, false));
            Assert.AreEqual(300, Payout.Coins(30, 2, 5, true));
            Assert.AreEqual(4, Payout.FreeSpins(2, 2, 3, false));
        }

        [Test]
        public void BigWinNeedsTheThreshold()
        {
            Assert.IsFalse(Payout.IsBigWin(4999, 5000));
            Assert.IsTrue(Payout.IsBigWin(5000, 5000));
            Assert.IsFalse(Payout.IsBigWin(0, 0));
        }
    }
}
