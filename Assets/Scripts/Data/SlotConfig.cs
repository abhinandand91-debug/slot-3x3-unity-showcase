using System.Collections.Generic;
using UnityEngine;

namespace Slot3x3
{
    public enum JackpotKind { None, Mini, Maxi, Mega }

    /// <summary>
    /// All of the tunable data of one slot machine: symbols, reel strips, pay table, spin
    /// feel, jackpot and bet limits. Everything the game logic needs lives here so a new
    /// machine is just a new asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Slot3x3/Slot Config", fileName = "SlotConfig")]
    public class SlotConfig : ScriptableObject
    {
        [Header("Symbols")]
        public SymbolDef[] symbols;
        public int scatterId = -1;
        public int wildId = -1;
        public bool useWild = true;
        public bool useScatter = true;
        [Tooltip("Allow the wild to replace the first symbol of a payline")]
        public bool wildInFirstPosition;

        [Header("Reels")]
        public ReelStrip[] reels;
        [Tooltip("Number of tiles around each reel cylinder")]
        public int tilesPerReel = 30;

        [Header("Pay table")]
        public List<PayLine> payTable = new List<PayLine>();
        public List<ScatterPay> scatterPayTable = new List<ScatterPay>();

        [Header("Spin feel")]
        [Tooltip("Seconds of the wind-up before the reel runs (0-1)")]
        public float inRotTime = 0.15f;
        [Tooltip("Degrees the reel winds back before it runs (0-10)")]
        public float inRotAngle = 7f;
        [Tooltip("Seconds of the settle after the reel stops (0-1)")]
        public float outRotTime = 0.15f;
        [Tooltip("Degrees the reel overshoots before it settles (0-10)")]
        public float outRotAngle = 7f;
        [Tooltip("Seconds of the main run")]
        public float mainRotateTime = 3f;
        [Tooltip("Random variation of the main run, percent (0-20)")]
        public int mainRotateTimeRandomize = 10;

        [Header("Betting")]
        public int startCoins = 500;
        public int maxLineBet = 20;
        public int defaultLineBet = 1;
        public bool multiplyWinByLineBet = true;
        public bool multiplyFreeSpinsByLineBet;

        [Header("Hold feature")]
        [Tooltip("Let the player hold reels so they stay put on the next spin")]
        public bool useHold = true;
        [Tooltip("How many reels can be held at once")]
        public int maxHeldReels = 2;
        [Tooltip("Total-bet multiplier by number of held reels: [0] none held, [1] one held, [2] two held ...")]
        public int[] holdBetMultipliers = { 1, 2, 4 };

        [Header("Daily reward")]
        [Tooltip("Coins for day 1, 2, 3 ... of the reward streak; after the last day it starts over")]
        public int[] dailyRewards = { 500, 1000, 1500, 2500, 4000, 6000, 10000 };
        [Tooltip("Hours between two rewards. Missing the next reward for a whole extra interval restarts the streak")]
        public float dailyRewardIntervalHours = 24f;

        [Header("Developer helpers")]
        [Tooltip("When the player cannot afford a bet, offer free coins (for testing; turn off for a real release)")]
        public bool allowDevRefill = true;
        public int devRefillCoins = 1000;
        [Tooltip("Coins added by the 'Add coins' button in the settings window (development builds only)")]
        public int devAddCoins = 5000;

        [Header("Progression")]
        public float loseSpinLevelProgress = 0.5f;
        public float winSpinLevelProgress = 2f;
        public int bigWinMinCoins = 5000;

        [Header("Jackpot")]
        public int jackpotSymbolId = -1;
        public bool useMegaJackpot = true;
        [Tooltip("Identical jackpot symbols on screen needed for the mega jackpot")]
        public int megaJackpotCount = 5;
        public int megaJackpotStart = 1000;
        [Tooltip("Coins added to the jackpot on every spin")]
        public int jackpotIncrement = 1;

        private List<PayLine> fullPayTable;

        /// <summary>The pay table extended with every wild-substituted variant.</summary>
        public List<PayLine> FullPayTable
        {
            get
            {
                if (fullPayTable == null) BuildFullPayTable();
                return fullPayTable;
            }
        }

        public void BuildFullPayTable()
        {
            fullPayTable = new List<PayLine>();
            foreach (PayLine line in payTable)
            {
                fullPayTable.Add(line);
                if (useWild) fullPayTable.AddRange(line.BuildWildVariants(this, reels.Length));
            }
        }

        private void OnEnable() { fullPayTable = null; }
        private void OnValidate() { fullPayTable = null; }
    }
}
