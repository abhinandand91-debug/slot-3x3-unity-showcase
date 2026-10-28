using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// A winning pattern: one symbol id per reel (-1 means "any symbol"), what it pays and
    /// how many free spins it awards.
    /// </summary>
    [Serializable]
    public class PayLine
    {
        public int[] symbols = { -1, -1, -1 };
        public int pay;
        public int freeSpins;
        [Tooltip("Multiplies the total win of the spin")]
        public int payMult = 1;
        [Tooltip("Multiplies the free spins won")]
        public int freeSpinsMult = 1;

        public PayLine() { }

        public PayLine(int[] symbols, int pay, int freeSpins = 0)
        {
            this.symbols = (int[])symbols.Clone();
            this.pay = pay;
            this.freeSpins = freeSpins;
        }

        public PayLine Clone()
        {
            return new PayLine(symbols, pay, freeSpins) { payMult = payMult, freeSpinsMult = freeSpinsMult };
        }

        public bool SameSymbols(PayLine other)
        {
            if (other == null || other.symbols.Length != symbols.Length) return false;
            for (int i = 0; i < symbols.Length; i++)
                if (symbols[i] != other.symbols[i]) return false;
            return true;
        }

        /// <summary>
        /// Builds every extra pattern in which one or more of this line's symbols are replaced
        /// by the wild. The first symbol of a line is never replaced unless
        /// <paramref name="wildInFirstPosition"/> is set, and symbols that opt out of wild
        /// substitution are left alone.
        /// </summary>
        public List<PayLine> BuildWildVariants(SlotConfig config, int reelCount)
        {
            var result = new List<PayLine>();

            // positions on the line where a wild is allowed to replace the symbol
            var positions = new List<int>();
            int seen = 0;
            for (int i = 0; i < symbols.Length && i < reelCount; i++)
            {
                int id = symbols[i];
                if (id < 0 || id == config.wildId) continue;
                if (!(seen == 0 && !config.wildInFirstPosition) && config.symbols[id].wildSubstitute)
                    positions.Add(i);
                seen++;
            }

            // every non-empty subset of those positions
            int subsets = 1 << positions.Count;
            for (int mask = 1; mask < subsets; mask++)
            {
                PayLine variant = Clone();
                for (int bit = 0; bit < positions.Count; bit++)
                    if ((mask & (1 << bit)) != 0) variant.symbols[positions[bit]] = config.wildId;

                if (!variant.SameSymbols(this) && !result.Exists(p => p.SameSymbols(variant)))
                    result.Add(variant);
            }
            return result;
        }
    }
}
