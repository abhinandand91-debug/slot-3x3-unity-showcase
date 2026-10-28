using System;
using System.Collections;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// The machine's control panel: line bet, selected lines, total bet, free spins, auto spin and the
    /// progressive jackpot, plus the text fields and buttons that show and change them.
    /// </summary>
    public class SlotControls : MonoBehaviour
    {
        private const string MegaJackpotKey = "slot3x3_mega_jackpot";

        [Header("Texts")]
        [SerializeField] private TextMesh balanceText;
        [SerializeField] private TextMesh winText;
        [SerializeField] private TextMesh totalBetText;
        [SerializeField] private TextMesh jackpotText;
        [SerializeField] private TextMesh freeSpinsLabel;
        [SerializeField] private TextMesh freeSpinsCountText;

        [Header("Buttons")]
        [SerializeField] private SceneButton spinButton;
        [SerializeField] private SceneButton autoSpinButton;
        [SerializeField] private SceneButton maxBetButton;
        [SerializeField] private SceneButton betPlusButton;
        [SerializeField] private SceneButton betMinusButton;
        [SerializeField] private SceneButton linesPlusButton;
        [SerializeField] private SceneButton linesMinusButton;

        [Header("Hold feature (optional)")]
        [SerializeField] private HoldFeature hold;

        [Header("References")]
        [SerializeField] private LinesController lines;

        private SlotConfig config;
        private SlotMachine machine;
        private PlayerWallet wallet;
        private SceneButton[] controlButtons;
        private Coroutine balanceTween;
        private Coroutine winTween;
        private float shownBalance;
        private float shownWin;

        public int LineBet { get; private set; }
        public int SelectedLines { get { return lines.SelectedCount; } }
        public int TotalBet { get { return LineBet * SelectedLines * HoldMultiplier; } }
        public HoldFeature Hold { get { return hold; } }
        public bool UseHold { get { return hold != null && hold.isActiveAndEnabled; } }
        public int HoldMultiplier { get { return UseHold ? hold.Multiplier : 1; } }
        public int FreeSpins { get; private set; }
        public bool HasFreeSpins { get { return FreeSpins > 0; } }
        public int MegaJackpot { get; private set; }
        public bool Auto { get; private set; }
        public int AutoSpinsCounter { get; private set; }
        public int AutoSpinLimit { get { return int.MaxValue; } }
        public bool AnyLineSelected { get { return SelectedLines > 0; } }

        public event Action<int> FreeSpinsChanged;

        public void Init(SlotConfig slotConfig, SlotMachine slotMachine, PlayerWallet playerWallet)
        {
            config = slotConfig;
            if (hold != null) { hold.gameObject.SetActive(config.useHold); if (config.useHold) hold.Init(config); }
            machine = slotMachine;
            wallet = playerWallet;

            controlButtons = new[] { autoSpinButton, maxBetButton, betPlusButton, betMinusButton, linesPlusButton, linesMinusButton };

            spinButton.Clicked += _ => OnSpin();
            autoSpinButton.Clicked += _ => OnAutoSpin();
            maxBetButton.Clicked += _ => OnMaxBet();
            betPlusButton.Clicked += _ => AddLineBet(1);
            betMinusButton.Clicked += _ => AddLineBet(-1);
            linesPlusButton.Clicked += _ => lines.SetSelectedCount(SelectedLines + 1, true);
            linesMinusButton.Clicked += _ => lines.SetSelectedCount(SelectedLines - 1, false);

            lines.SelectionChanged += (count, burn) => RefreshBet();
            if (UseHold) hold.MultiplierChanged += _ => RefreshBet();
            wallet.CoinsChanged += OnCoinsChanged;
            wallet.WinCoinsChanged += OnWinChanged;

            MegaJackpot = Mathf.Max(config.megaJackpotStart, GameServices.Save.GetInt(MegaJackpotKey, config.megaJackpotStart));
            LineBet = Mathf.Clamp(config.defaultLineBet, 1, config.maxLineBet);

            shownBalance = wallet.Coins;
            balanceText.text = NumberFormat.Coins(wallet.Coins);
            winText.text = "0";
            jackpotText.text = NumberFormat.Coins(MegaJackpot);
            RefreshBet();
            RefreshFreeSpins();
        }

        #region activity
        /// <summary>Enables or disables the control buttons; the spin button has its own flag.</summary>
        public void SetControlActivity(bool controls, bool spin)
        {
            foreach (SceneButton button in controlButtons) button.interactable = controls;
            spinButton.interactable = spin;
            lines.SetInteractable(controls);
            if (UseHold) hold.SetInteractable(controls);
        }
        #endregion activity

        #region button handlers
        private void OnSpin()
        {
            if (Auto)
            {
                ResetAutoMode();
                return;
            }
            machine.SpinPressed();
        }

        private void OnAutoSpin()
        {
            if (Auto)
            {
                ResetAutoMode();
                return;
            }
            AutoSpinsCounter = 0;
            Auto = true;
            machine.SpinPressed();
        }

        private void OnMaxBet()
        {
            lines.SelectAll(true);
            SetLineBet(config.maxLineBet);
        }
        #endregion button handlers

        #region bet
        public void AddLineBet(int delta)
        {
            SetLineBet(LineBet + delta);
        }

        private void SetLineBet(int value)
        {
            LineBet = Mathf.Clamp(value, 1, config.maxLineBet);
            RefreshBet();
        }

        private void RefreshBet()
        {
            totalBetText.text = "TOTAL BET " + NumberFormat.Coins(TotalBet);
        }

        /// <summary>Takes the total bet from the player's balance; false if they cannot afford it.</summary>
        public bool ApplyBet()
        {
            if (!wallet.HasMoneyForBet(TotalBet)) return false;
            wallet.AddCoins(-TotalBet);
            return true;
        }
        #endregion bet

        #region free spins
        public void AddFreeSpins(int count)
        {
            FreeSpins = Mathf.Max(0, FreeSpins + count);
            RefreshFreeSpins();
            FreeSpinsChanged?.Invoke(FreeSpins);
        }

        /// <summary>Uses one free spin if there is any.</summary>
        public bool ApplyFreeSpin()
        {
            if (!HasFreeSpins) return false;
            AddFreeSpins(-1);
            return true;
        }

        private void RefreshFreeSpins()
        {
            freeSpinsLabel.text = HasFreeSpins ? "Free Spins:" : "";
            freeSpinsCountText.text = HasFreeSpins ? FreeSpins.ToString() : "";
        }
        #endregion free spins

        #region auto spin
        public void IncrementAutoSpins()
        {
            AutoSpinsCounter++;
        }

        public void ResetAutoMode()
        {
            Auto = false;
            autoSpinButton.Release();
        }
        #endregion auto spin

        #region jackpot
        /// <summary>Grows the progressive jackpot; called once per spin.</summary>
        public void IncreaseJackpot()
        {
            if (config.useMegaJackpot) SetMegaJackpot(MegaJackpot + config.jackpotIncrement);
        }

        public void ResetJackpot()
        {
            SetMegaJackpot(config.megaJackpotStart);
        }

        private void SetMegaJackpot(int value)
        {
            MegaJackpot = Mathf.Max(config.megaJackpotStart, value);
            GameServices.Save.SetInt(MegaJackpotKey, MegaJackpot);
            jackpotText.text = NumberFormat.Coins(MegaJackpot);
        }

        public int GetJackpotCoins(JackpotKind kind)
        {
            return kind == JackpotKind.Mega ? MegaJackpot : 0;
        }
        #endregion jackpot

        #region displayed numbers
        private void OnCoinsChanged(int coins)
        {
            if (balanceTween != null) StopCoroutine(balanceTween);
            balanceTween = StartCoroutine(CountTo(shownBalance, coins, 1f, v => { shownBalance = v; balanceText.text = NumberFormat.Coins(Mathf.RoundToInt(v)); }));
        }

        private void OnWinChanged(int coins)
        {
            if (winTween != null) StopCoroutine(winTween);
            winTween = StartCoroutine(CountTo(shownWin, coins, 0.5f, v => { shownWin = v; winText.text = NumberFormat.Coins(Mathf.RoundToInt(v)); }));
        }

        private static IEnumerator CountTo(float from, float to, float duration, Action<float> apply)
        {
            return Tween.Value(from, to, duration, apply, Ease.OutSine);
        }
        #endregion displayed numbers
    }
}
