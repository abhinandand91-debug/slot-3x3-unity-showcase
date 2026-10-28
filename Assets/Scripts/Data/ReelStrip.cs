using System;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// The ordered "tape" of symbol ids that a single reel cycles through, plus the
    /// per-reel spin feel (start delay and how many full laps it makes before stopping).
    /// </summary>
    [Serializable]
    public class ReelStrip
    {
        [Tooltip("Symbol ids in the order they appear on the reel, bottom to top")]
        public int[] order;
        [Tooltip("Seconds this reel waits before it starts spinning (gives the left-to-right stagger)")]
        public float spinStartDelay;
        [Tooltip("Number of extra full laps the reel makes before it stops")]
        public int laps = 3;
        [Tooltip("Start the reel on a random position")]
        public bool randomStart = true;

        public int Length { get { return order == null ? 0 : order.Length; } }
    }
}
