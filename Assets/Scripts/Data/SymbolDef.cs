using System;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>One symbol that can appear on the reels.</summary>
    [Serializable]
    public class SymbolDef
    {
        public string name;
        public Sprite sprite;
        [Tooltip("Sprite shown while the reel is spinning fast")]
        public Sprite blur;
        [Tooltip("Can a wild symbol stand in for this one in a payline?")]
        public bool wildSubstitute = true;
    }
}
