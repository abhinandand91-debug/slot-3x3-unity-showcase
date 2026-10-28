using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// One of the machine's paylines. It knows which window row it crosses on every reel,
    /// owns the selector button next to the reels, draws itself, and checks whether the
    /// symbols that stopped on it form a winning combination.
    /// </summary>
    public class PaylineView : MonoBehaviour
    {
        [SerializeField, Tooltip("Line number shown on the selector button, starting at 1")]
        private int number = 1;
        [SerializeField, Tooltip("For each reel, the window row the line passes through (0 = top)")]
        private int[] rowPerReel;
        [SerializeField] private SceneButton selectorButton;
        [SerializeField] private TextMesh winText;

        private Reel[] reels;
        private LineStroke stroke;

        public int Number { get { return number; } }
        public bool IsSelected { get; private set; }
        public LineWin Win { get; private set; }
        public bool IsWinning { get { return Win != null; } }
        public SceneButton SelectorButton { get { return selectorButton; } }

        /// <summary>Raised when the selector button is pressed.</summary>
        public event Action<PaylineView> SelectorPressed;

        public void Setup(int lineNumber, int[] rows, SceneButton button, TextMesh text)
        {
            number = lineNumber;
            rowPerReel = rows;
            selectorButton = button;
            winText = text;
        }

        public void Init(Reel[] slotReels, Sprite lineSprite, Sprite dotSprite, int sortingOrder)
        {
            reels = slotReels;

            var points = new List<Vector3>();
            for (int i = 0; i < reels.Length; i++)
                points.Add(reels[i].RowAnchor(rowPerReel[i]).position);

            var strokeObject = new GameObject("Stroke");
            strokeObject.transform.SetParent(transform, true);
            stroke = strokeObject.AddComponent<LineStroke>();
            stroke.Build(points, lineSprite, dotSprite, sortingOrder, null);

            if (winText != null) winText.gameObject.SetActive(false);
            if (selectorButton != null) selectorButton.Clicked += OnSelectorClicked;
        }

        private void OnDestroy()
        {
            if (selectorButton != null) selectorButton.Clicked -= OnSelectorClicked;
        }

        private void OnSelectorClicked(SceneButton button)
        {
            SelectorPressed?.Invoke(this);
        }

        #region selection
        public void Select(bool burn, float burnDelay)
        {
            IsSelected = true;
            if (burn) stroke.Burn(burnDelay); else stroke.CancelBurn();
            if (selectorButton != null) selectorButton.SetPressedVisual(true);
        }

        public void Deselect()
        {
            IsSelected = false;
            stroke.CancelBurn();
            if (selectorButton != null) selectorButton.SetPressedVisual(false);
        }

        public void SetInteractable(bool value)
        {
            if (selectorButton != null) selectorButton.interactable = value;
        }
        #endregion selection

        #region drawing
        public void SetVisible(bool visible) { stroke.SetVisible(visible); }

        public void Flash(bool flashing)
        {
            stroke.Flash(flashing);
            if (winText == null) return;

            bool show = flashing && IsWinning && Win.Pay > 0;
            winText.gameObject.SetActive(show);
            if (show)
            {
                // the amount sits on the middle of the line
                int middle = reels.Length / 2;
                Vector3 position = reels.Length % 2 == 1
                    ? reels[middle].RowAnchor(rowPerReel[middle]).position
                    : (reels[middle].RowAnchor(rowPerReel[middle]).position +
                       reels[middle - 1].RowAnchor(rowPerReel[middle - 1]).position) * 0.5f;
                winText.transform.position = new Vector3(position.x, position.y, winText.transform.position.z);
                winText.text = Win.Pay.ToString();
            }
        }

        public void HideAll()
        {
            stroke.Flash(false);
            stroke.CancelBurn();
            if (winText != null) winText.gameObject.SetActive(false);
        }
        #endregion drawing

        #region win search
        /// <summary>Looks for the best paying pattern of the pay table on this line.</summary>
        public void FindWin(List<PayLine> payTable)
        {
            Win = null;

            var ids = new int[reels.Length];
            for (int i = 0; i < reels.Length; i++)
                ids[i] = reels[i].GetSymbolId(rowPerReel[i]);

            foreach (PayLine pattern in payTable)
            {
                if (!Matches(pattern, ids)) continue;

                if (Win == null || Win.Pay < pattern.pay || Win.FreeSpins < pattern.freeSpins)
                {
                    var tiles = new List<SlotTile>();
                    for (int i = 0; i < reels.Length; i++)
                        if (pattern.symbols[i] >= 0) tiles.Add(reels[i].GetTile(rowPerReel[i]));
                    Win = new LineWin(tiles, pattern.pay, pattern.freeSpins, pattern.payMult, pattern.freeSpinsMult);
                }
            }
        }

        private static bool Matches(PayLine pattern, int[] ids)
        {
            if (pattern.symbols.Length < ids.Length) return false;
            for (int i = 0; i < ids.Length; i++)
                if (pattern.symbols[i] >= 0 && pattern.symbols[i] != ids[i]) return false;
            return true;
        }

        public void ResetWin()
        {
            Win = null;
        }
        #endregion win search
    }
}
