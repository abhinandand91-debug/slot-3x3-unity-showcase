using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Slot3x3.EditorTools
{
    /// <summary>Creates the SlotConfig asset for the classic 3x3 machine: symbols, reel strips and pay table.</summary>
    public static class ClassicConfigFactory
    {
        public const string ConfigPath = "Assets/Data/ClassicSlotConfig.asset";

        // symbol ids, in the order of the SlotConfig.symbols list
        public const int A = 0, J = 1, K = 2, Q = 3, Ten = 4, Seven = 5, Bar1 = 6, Bar2 = 7, Bar3 = 8,
                         Bell = 9, FreeSpin = 10, Scatter = 11, Wild = 12, Jackpot = 13;

        public static SlotConfig CreateOrUpdate()
        {
            EnsureFolder("Assets/Data");
            var config = AssetDatabase.LoadAssetAtPath<SlotConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SlotConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }

            config.symbols = new[]
            {
                Symbol("A", "A.png", "Symbol Blur A.png", true),
                Symbol("J", "J.png", "Symbol Blur J.png", true),
                Symbol("K", "K.png", "Symbol Blur K.png", true),
                Symbol("Q", "Q.png", "Symbol Blur Q.png", true),
                Symbol("10", "10.png", "Symbol Blur 10.png", true),
                Symbol("7", "7.png", "Symbol Blur 7.png", true),
                Symbol("1Bar", "1Bar.png", "Symbol Blur Bar.png", true),
                Symbol("2Bar", "2Bar.png", "Symbol Blur 2Bar.png", true),
                Symbol("3Bar", "3Bar.png", "Symbol Blur 3Bar.png", true),
                Symbol("Bell", "Bell.png", "Symbol Blur Bell.png", true),
                Symbol("FreeSpin", "FreeSpin.png", "Symbol Blur Free Speen.png", false),
                Symbol("Scatter", "Scatter.png", "Symbol Blur Scatter.png", false),
                Symbol("Wild", "Wild.png", "Symbol Blur Wild.png", false),
                Symbol("Jackpot", "Jackpot.png", "Symbol Blur Jackpot.png", false),
            };
            config.scatterId = Scatter;
            config.wildId = Wild;
            config.useWild = true;
            config.useScatter = true;
            config.wildInFirstPosition = false;

            config.tilesPerReel = 30;
            config.reels = new[]
            {
                Reel(0.1f, A, J, K, Q, J, Bar2, Ten, Seven, Bell, Bar1, Bar3, FreeSpin, Scatter, Wild, Jackpot, Ten, Bell),
                Reel(0.2f, A, Bar2, J, K, Q, Ten, Bar1, Bell, FreeSpin, A, Scatter, Wild, Jackpot, Ten, Jackpot, Seven, Bar3, A),
                Reel(0.3f, A, J, Bar3, K, Q, Bar2, Ten, Seven, Bar1, Bell, FreeSpin, A, Scatter, Wild, Jackpot, Ten, Jackpot, A),
            };

            config.payTable = new List<PayLine>
            {
                Line(A, 10), Line(J, 15), Line(K, 20), Line(Q, 25), Line(Ten, 30),
                Line(Bar1, 120), Line(Bar2, 130), Line(Bar3, 150), Line(Seven, 160), Line(Bell, 170),
                Line(FreeSpin, 0, 3), Line(Wild, 200),
            };
            config.scatterPayTable = new List<ScatterPay>
            {
                // five scatters anywhere on screen fill the level bar
                new ScatterPay { scattersCount = 5, levelProgress = 100f },
            };

            config.inRotTime = 0.15f; config.inRotAngle = 7f;
            config.outRotTime = 0.15f; config.outRotAngle = 7f;
            config.mainRotateTime = 3f; config.mainRotateTimeRandomize = 10;

            config.startCoins = 500;
            config.maxLineBet = 20;
            config.defaultLineBet = 1;
            config.multiplyWinByLineBet = true;
            config.multiplyFreeSpinsByLineBet = false;

            config.loseSpinLevelProgress = 0.5f;
            config.winSpinLevelProgress = 2f;
            config.bigWinMinCoins = 5000;

            config.jackpotSymbolId = Jackpot;
            config.useMegaJackpot = true;
            config.megaJackpotCount = 5;
            config.megaJackpotStart = 1000;
            config.jackpotIncrement = 1;

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static SymbolDef Symbol(string name, string spriteFile, string blurFile, bool wildSubstitute)
        {
            return new SymbolDef
            {
                name = name,
                sprite = SymbolArt.Get(spriteFile),
                blur = SymbolArt.Get(blurFile),
                wildSubstitute = wildSubstitute,
            };
        }

        private static ReelStrip Reel(float startDelay, params int[] order)
        {
            return new ReelStrip { order = order, spinStartDelay = startDelay, laps = 3, randomStart = true };
        }

        /// <summary>Three of the same symbol across the reels.</summary>
        private static PayLine Line(int symbol, int pay, int freeSpins = 0)
        {
            return new PayLine(new[] { symbol, symbol, symbol }, pay, freeSpins);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
