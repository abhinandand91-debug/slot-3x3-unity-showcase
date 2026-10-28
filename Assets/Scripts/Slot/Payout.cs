namespace Slot3x3
{
    /// <summary>
    /// The payout rules as plain functions with no Unity objects, so they are easy to read and to unit test.
    /// <see cref="SlotMachine"/> calls these after <see cref="WinEvaluator"/> has found the wins.
    /// </summary>
    public static class Payout
    {
        /// <summary>Coins for a spin: the line wins times the multiplier, optionally times the line bet.</summary>
        public static int Coins(int lineWinCoins, int multiplier, int lineBet, bool scaleByLineBet)
        {
            int coins = lineWinCoins * multiplier;
            return scaleByLineBet ? coins * lineBet : coins;
        }

        /// <summary>Free spins for a spin, with the same optional line-bet scaling as coins.</summary>
        public static int FreeSpins(int wonFreeSpins, int multiplier, int lineBet, bool scaleByLineBet)
        {
            int spins = wonFreeSpins * multiplier;
            return scaleByLineBet ? spins * lineBet : spins;
        }

        /// <summary>A win big enough to get the full-screen celebration.</summary>
        public static bool IsBigWin(int coins, int bigWinMinCoins)
        {
            return coins > 0 && coins >= bigWinMinCoins;
        }
    }
}
