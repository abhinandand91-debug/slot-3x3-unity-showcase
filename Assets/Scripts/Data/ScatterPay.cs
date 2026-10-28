using System;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>Reward for having an exact number of scatter symbols anywhere on screen.</summary>
    [Serializable]
    public class ScatterPay
    {
        public int scattersCount = 3;
        public int pay;
        public int freeSpins;
        public int payMult = 1;
        public int freeSpinsMult = 1;
        [Tooltip("Level progress (percent of a level) awarded together with the win")]
        public float levelProgress;
    }
}
