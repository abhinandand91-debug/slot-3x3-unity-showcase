using System.Collections.Generic;

namespace Slot3x3
{
    /// <summary>
    /// Looks at the symbols that stopped in the window and works out what they win: payline wins,
    /// the scatter and the jackpot, and the totals that the slot machine pays out.
    /// </summary>
    public class WinEvaluator
    {
        private readonly SlotConfig config;
        private readonly Reel[] reels;
        private readonly LinesController lines;

        public LineWin ScatterWin { get; private set; }
        public List<SlotTile> ScatterTiles { get; private set; } = new List<SlotTile>();
        public List<SlotTile> JackpotTiles { get; private set; } = new List<SlotTile>();
        public JackpotKind Jackpot { get; private set; }

        public WinEvaluator(SlotConfig config, Reel[] reels, LinesController lines)
        {
            this.config = config;
            this.reels = reels;
            this.lines = lines;
        }

        public void Reset()
        {
            lines.ResetWins();
            ScatterWin = null;
            ScatterTiles = new List<SlotTile>();
            JackpotTiles = new List<SlotTile>();
            Jackpot = JackpotKind.None;
        }

        /// <summary>Checks paylines, scatter and jackpot for the symbols currently in the window.</summary>
        public void Search()
        {
            Reset();
            lines.FindWins(config.FullPayTable);
            if (config.useScatter) SearchScatter();
            SearchJackpot();
        }

        public bool HasAnyWin
        {
            get { return lines.AnyWinningLine || ScatterWin != null || Jackpot != JackpotKind.None; }
        }

        #region scatter and jackpot
        private void SearchScatter()
        {
            ScatterTiles = TilesWithSymbol(config.scatterId);
            foreach (ScatterPay pay in config.scatterPayTable)
            {
                if (pay.scattersCount > 0 && pay.scattersCount == ScatterTiles.Count)
                    ScatterWin = new LineWin(ScatterTiles, pay.pay, pay.freeSpins, pay.payMult, pay.freeSpinsMult, pay.levelProgress);
            }
            if (ScatterWin == null) ScatterTiles = new List<SlotTile>();
        }

        private void SearchJackpot()
        {
            if (!config.useMegaJackpot || config.jackpotSymbolId < 0) return;

            List<SlotTile> tiles = TilesWithSymbol(config.jackpotSymbolId);
            if (config.megaJackpotCount > 0 && tiles.Count >= config.megaJackpotCount)
            {
                Jackpot = JackpotKind.Mega;
                JackpotTiles = tiles;
            }
        }

        private List<SlotTile> TilesWithSymbol(int symbolId)
        {
            var tiles = new List<SlotTile>();
            foreach (Reel reel in reels)
            {
                for (int row = 0; row < reel.Rows; row++)
                    if (reel.GetSymbolId(row) == symbolId) tiles.Add(reel.GetTile(row));
            }
            return tiles;
        }
        #endregion scatter and jackpot

        #region totals
        public int WinCoins
        {
            get
            {
                int total = 0;
                foreach (PaylineView line in lines.Lines)
                    if (line.IsWinning) total += line.Win.Pay;
                if (ScatterWin != null) total += ScatterWin.Pay;
                return total;
            }
        }

        public int WinFreeSpins
        {
            get
            {
                int total = 0;
                foreach (PaylineView line in lines.Lines)
                    if (line.IsWinning) total += line.Win.FreeSpins;
                if (ScatterWin != null) total += ScatterWin.FreeSpins;
                return total;
            }
        }

        public int PayMultiplier
        {
            get
            {
                int product = 1;
                foreach (PaylineView line in lines.Lines)
                    if (line.IsWinning && line.Win.PayMult > 0) product *= line.Win.PayMult;
                if (ScatterWin != null && ScatterWin.PayMult > 0) product *= ScatterWin.PayMult;
                return product;
            }
        }

        public int FreeSpinsMultiplier
        {
            get
            {
                int product = 1;
                foreach (PaylineView line in lines.Lines)
                    if (line.IsWinning && line.Win.FreeSpinsMult != 0) product *= line.Win.FreeSpinsMult;
                if (ScatterWin != null && ScatterWin.FreeSpinsMult > 0) product *= ScatterWin.FreeSpinsMult;
                return product;
            }
        }

        public int WinLinesCount
        {
            get
            {
                int count = 0;
                foreach (PaylineView line in lines.Lines)
                    if (line.IsWinning) count++;
                return count;
            }
        }
        #endregion totals
    }
}
