using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// The hold feature: under every reel is a toggle button. A held reel keeps its symbols on the next spin.
    /// Holding costs extra - the total bet is multiplied depending on how many reels are held.
    /// </summary>
    public class HoldFeature : MonoBehaviour
    {
        [SerializeField] private SceneButton[] holdButtons;
        [SerializeField] private TextMesh[] multiplierTexts;

        private int maxHeld = 2;
        private int[] multipliers = { 1, 2, 4 };
        private readonly List<SceneButton> held = new List<SceneButton>();

        public int Multiplier { get; private set; } = 1;

        /// <summary>Raised with the new bet multiplier whenever the set of held reels changes.</summary>
        public event Action<int> MultiplierChanged;

        public void Setup(SceneButton[] buttons, TextMesh[] texts)
        {
            holdButtons = buttons;
            multiplierTexts = texts;
        }

        public void Init(SlotConfig config)
        {
            maxHeld = Mathf.Max(0, config.maxHeldReels);
            multipliers = config.holdBetMultipliers;
            foreach (SceneButton button in holdButtons) button.Clicked += OnHoldClicked;
            RefreshTexts();
        }

        private void OnDestroy()
        {
            if (holdButtons == null) return;
            foreach (SceneButton button in holdButtons)
                if (button != null) button.Clicked -= OnHoldClicked;
        }

        public bool IsHeld(int reel)
        {
            return holdButtons[reel].Pressed;
        }

        public void SetInteractable(bool value)
        {
            foreach (SceneButton button in holdButtons) button.interactable = value;
        }

        /// <summary>Bet multiplier for a number of held reels (the last table entry is used for larger counts).</summary>
        public static int MultiplierFor(int heldCount, int[] table)
        {
            if (table == null || table.Length == 0 || heldCount <= 0) return 1;
            return Mathf.Max(1, table[Mathf.Min(heldCount, table.Length - 1)]);
        }

        private void OnHoldClicked(SceneButton button)
        {
            if (!button.Pressed)
            {
                held.Remove(button);
            }
            else if (held.Count < maxHeld)
            {
                held.Add(button);
            }
            else if (held.Count > 0)
            {
                // too many: the earliest hold is released in favour of the new one
                SceneButton oldest = held[0];
                oldest.Release();
                held.RemoveAt(0);
                held.Add(button);
            }
            else
            {
                button.Release(); // holding is not allowed at all
            }

            Multiplier = MultiplierFor(held.Count, multipliers);
            RefreshTexts();
            MultiplierChanged?.Invoke(Multiplier);
        }

        private void RefreshTexts()
        {
            foreach (TextMesh text in multiplierTexts)
                if (text != null) text.text = Multiplier.ToString();
        }
    }
}
