using NUnit.Framework;
using UnityEditor;

namespace Slot3x3.Tests
{
    public class PayTableTests
    {
        private SlotConfig config;

        [SetUp]
        public void LoadConfig()
        {
            config = AssetDatabase.LoadAssetAtPath<SlotConfig>("Assets/Data/ClassicSlotConfig.asset");
            config.BuildFullPayTable();
        }

        private static int Id(SlotConfig c, string name)
        {
            return System.Array.FindIndex(c.symbols, s => s.name == name);
        }

        [Test]
        public void WildReplacesEverySubsetExceptTheFirstSymbol()
        {
            int a = Id(config, "A");
            var line = new PayLine(new[] { a, a, a }, 10);

            var variants = line.BuildWildVariants(config, 3);

            Assert.AreEqual(3, variants.Count);
            Assert.IsTrue(variants.Exists(v => v.symbols[0] == a && v.symbols[1] == config.wildId && v.symbols[2] == a));
            Assert.IsTrue(variants.Exists(v => v.symbols[0] == a && v.symbols[1] == a && v.symbols[2] == config.wildId));
            Assert.IsTrue(variants.Exists(v => v.symbols[0] == a && v.symbols[1] == config.wildId && v.symbols[2] == config.wildId));
            Assert.IsFalse(variants.Exists(v => v.symbols[0] == config.wildId), "wild must not replace the first symbol");
            Assert.IsTrue(variants.TrueForAll(v => v.pay == 10));
        }

        [Test]
        public void SymbolsThatOptOutOfWildGetNoVariants()
        {
            int freeSpin = Id(config, "FreeSpin");
            var line = new PayLine(new[] { freeSpin, freeSpin, freeSpin }, 0, 3);
            Assert.AreEqual(0, line.BuildWildVariants(config, 3).Count);
        }

        [Test]
        public void AWildLineHasNoVariants()
        {
            var line = new PayLine(new[] { config.wildId, config.wildId, config.wildId }, 200);
            Assert.AreEqual(0, line.BuildWildVariants(config, 3).Count);
        }

        [Test]
        public void FullPayTableHasBaseLinesPlusThreeWildVariantsForEachOfTheTenNormalSymbols()
        {
            Assert.AreEqual(12, config.payTable.Count);
            Assert.AreEqual(12 + 10 * 3, config.FullPayTable.Count);
        }

        [Test]
        public void ReelStripsOnlyUseExistingSymbols()
        {
            foreach (ReelStrip strip in config.reels)
                foreach (int id in strip.order)
                    Assert.That(id, Is.InRange(0, config.symbols.Length - 1));
        }

        [Test]
        public void EverySymbolHasSpriteAndBlur()
        {
            foreach (SymbolDef symbol in config.symbols)
            {
                Assert.IsNotNull(symbol.sprite, symbol.name);
                Assert.IsNotNull(symbol.blur, symbol.name);
            }
        }
    }
}
