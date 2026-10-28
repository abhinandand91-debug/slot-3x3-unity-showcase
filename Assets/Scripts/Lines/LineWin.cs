using System.Collections.Generic;

namespace Slot3x3
{
    /// <summary>The result of a winning payline (or the scatter): which tiles won and what they pay.</summary>
    public class LineWin
    {
        public readonly List<SlotTile> Tiles;
        public readonly int Pay;
        public readonly int FreeSpins;
        public readonly int PayMult;
        public readonly int FreeSpinsMult;
        public readonly float LevelProgress;

        public LineWin(List<SlotTile> tiles, int pay, int freeSpins, int payMult, int freeSpinsMult, float levelProgress = 0f)
        {
            Tiles = tiles;
            Pay = pay;
            FreeSpins = freeSpins;
            PayMult = payMult;
            FreeSpinsMult = freeSpinsMult;
            LevelProgress = levelProgress;
        }

        public override string ToString()
        {
            return "Pay: " + Pay + " ; FreeSpins: " + FreeSpins + " ; PayMult: " + PayMult + " ; FreeSpinsMult: " + FreeSpinsMult;
        }
    }
}
