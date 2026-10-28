using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// Owns all paylines. Pressing line N on the machine selects lines 1..N, pressing a selected
    /// line deselects it and every line after it.
    /// </summary>
    public class LinesController : MonoBehaviour
    {
        [SerializeField] private PaylineView[] lines;
        [SerializeField] private Sprite lineSprite;
        [SerializeField] private Sprite dotSprite;
        [SerializeField] private int strokeSortingOrder = 130;

        public IReadOnlyList<PaylineView> Lines { get { return lines; } }
        public int LinesCount { get { return lines.Length; } }
        public int SelectedCount { get; private set; }

        /// <summary>Raised with the new number of selected lines and whether to flash them.</summary>
        public event Action<int, bool> SelectionChanged;

        public void Setup(PaylineView[] paylines, Sprite line, Sprite dot)
        {
            lines = paylines;
            lineSprite = line;
            dotSprite = dot;
        }

        public void Init(Reel[] reels)
        {
            Array.Sort(lines, (a, b) => a.Number.CompareTo(b.Number));
            foreach (PaylineView line in lines)
            {
                line.Init(reels, lineSprite, dotSprite, strokeSortingOrder);
                line.SelectorPressed += OnSelectorPressed;
            }
        }

        private void OnDestroy()
        {
            foreach (PaylineView line in lines)
                if (line != null) line.SelectorPressed -= OnSelectorPressed;
        }

        public void SetInteractable(bool value)
        {
            foreach (PaylineView line in lines) line.SetInteractable(value);
        }

        #region selection
        public void SelectAll(bool burn)
        {
            SetSelectedCount(lines.Length, burn);
        }

        public void SetSelectedCount(int count, bool burn)
        {
            count = Mathf.Clamp(count, 1, lines.Length);
            bool changed = count != SelectedCount;
            SelectedCount = count;

            for (int i = 0; i < lines.Length; i++)
            {
                if (i < count)
                {
                    if (!lines[i].IsSelected) lines[i].Select(burn, 0f);
                }
                else if (lines[i].IsSelected)
                {
                    lines[i].Deselect();
                }
            }

            if (changed) SelectionChanged?.Invoke(count, burn);
        }

        private void OnSelectorPressed(PaylineView line)
        {
            SetSelectedCount(line.IsSelected ? line.Number - 1 : line.Number, true);
        }
        #endregion selection

        #region win helpers
        public void ResetWins()
        {
            foreach (PaylineView line in lines) line.ResetWin();
        }

        public void HideAll()
        {
            foreach (PaylineView line in lines) line.HideAll();
        }

        public void FindWins(List<PayLine> payTable)
        {
            foreach (PaylineView line in lines)
                if (line.IsSelected) line.FindWin(payTable);
        }

        public bool AnyWinningLine
        {
            get
            {
                foreach (PaylineView line in lines)
                    if (line.IsWinning) return true;
                return false;
            }
        }
        #endregion win helpers
    }
}
