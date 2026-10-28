using System.Collections;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// The heart of the game. It runs one spin from start to finish: take the bet, spin the reels, look
    /// for wins, pay them out, show them, and (for auto spin and free spins) go round again.
    /// </summary>
    public class SlotMachine : MonoBehaviour
    {
        private const float JackpotShowSeconds = 3f;
        private const float JackpotShowAutoSeconds = 5f;
        private const float BigWinShowSeconds = 3f;

        [SerializeField] private SlotConfig config;
        [SerializeField] private Reel[] reels;
        [SerializeField] private SlotTile tilePrefab;
        [SerializeField] private LinesController lines;
        [SerializeField] private SlotControls controls;
        [SerializeField] private WinPresenter winPresenter;
        [SerializeField] private JackpotPresenter jackpotPresenter;
        [SerializeField] private BigWinPopup bigWinPopup;
        [SerializeField] private MessageToast toast;
        [SerializeField] private PlayerWallet wallet;
        [SerializeField] private DailyRewardController dailyReward;
        [SerializeField] private OutOfCreditsPopup outOfCredits;
        [SerializeField] private SettingsPopup settings;
        [SerializeField, Tooltip("Testing aid: stop the reels on these strip positions instead of random ones (leave empty for normal play)")]
        private int[] debugStops;

        private WinEvaluator evaluator;
        private bool spinning;
        private bool playFreeSpins;
        private Coroutine spinRoutine;

        private ISoundPlayer Sound { get { return GameServices.Sound; } }

        private void Start()
        {
            for (int i = 0; i < reels.Length; i++) reels[i].Build(config, i, tilePrefab);

            lines.Init(reels);
            evaluator = new WinEvaluator(config, reels, lines);
            winPresenter.Init(evaluator);
            controls.Init(config, this, wallet);
            wallet.Announce();

            outOfCredits.Init(config, wallet);
            dailyReward.Init(config, wallet);
            settings.InitDev(config, wallet, dailyReward);

            lines.SelectAll(true);
            SetInputActivity(true);
        }

        /// <summary>Called by the spin and auto buttons.</summary>
        public void SpinPressed()
        {
            StartSpin();
        }

        private void StartSpin()
        {
            if (spinning) return;

            // leave the previous result behind
            winPresenter.Cancel();
            winPresenter.ShowEffects(false, false);
            evaluator.Reset();
            jackpotPresenter.Cancel();

            if (!controls.AnyLineSelected)
            {
                toast.Show("Please select any line.", 1.5f);
                controls.ResetAutoMode();
                return;
            }
            if (!controls.ApplyFreeSpin() && !controls.ApplyBet())
            {
                controls.ResetAutoMode();
                if (config.allowDevRefill) outOfCredits.Open();
                else toast.Show("You have no money.", 1.5f);
                return;
            }
            if (controls.Auto && controls.AutoSpinsCounter >= controls.AutoSpinLimit)
            {
                controls.ResetAutoMode();
                return;
            }

            if (spinRoutine != null) StopCoroutine(spinRoutine);
            spinRoutine = StartCoroutine(SpinRoutine());
        }

        private IEnumerator SpinRoutine()
        {
            spinning = true;
            if (controls.Auto) controls.IncrementAutoSpins();

            wallet.SetWinCoins(0);
            SetInputActivity(false);
            lines.HideAll();

            // 1. spin the reels and wait for all of them to stop
            Sound.PlayRotation();
            int stopped = 0;
            bool forced = debugStops != null && debugStops.Length == reels.Length;
            for (int i = 0; i < reels.Length; i++)
            {
                if (controls.UseHold && controls.Hold.IsHeld(i)) { stopped++; continue; } // held reels stay where they are
                reels[i].Spin(forced ? debugStops[i] : reels[i].PickRandomStop(), () => stopped++);
            }
            while (stopped < reels.Length) yield return null;
            Sound.StopRotation();

            // 2. look at what landed
            evaluator.Search();
            controls.IncreaseJackpot();

            if (evaluator.HasAnyWin) yield return WinRoutine();
            else LoseResult();

            while (bigWinPopup.IsOpen) yield return null;

            if (controls.Auto || playFreeSpins) StartSpin();
        }

        #region results
        private void LoseResult()
        {
            Sound.PlayLose();
            wallet.AddLevelProgress(config.loseSpinLevelProgress);

            playFreeSpins = controls.HasFreeSpins;
            spinning = false;
            SetInputActivity(true);
            Sound.PlayMusic();
        }

        private IEnumerator WinRoutine()
        {
            winPresenter.ShowEffects(false, true);

            // jackpot
            int jackpotCoins = 0;
            if (evaluator.Jackpot != JackpotKind.None)
            {
                jackpotCoins = controls.GetJackpotCoins(evaluator.Jackpot);
                if (jackpotCoins > 0)
                {
                    wallet.AddCoins(jackpotCoins);
                    jackpotPresenter.Show();
                    yield return new WaitForSeconds(controls.HasFreeSpins || controls.Auto ? JackpotShowAutoSeconds : JackpotShowSeconds);
                    controls.ResetJackpot();
                }
            }

            // coins
            int winCoins = Payout.Coins(evaluator.WinCoins, evaluator.PayMultiplier, controls.LineBet, config.multiplyWinByLineBet);
            wallet.SetWinCoins(winCoins);
            wallet.AddCoins(winCoins);

            if (Payout.IsBigWin(winCoins, config.bigWinMinCoins))
            {
                bigWinPopup.Show(winCoins, BigWinShowSeconds);
                while (bigWinPopup.IsOpen) yield return null;
            }
            else if (winCoins > 0)
            {
                Sound.PlayWinCoins();
            }

            // free spins
            int winSpins = Payout.FreeSpins(evaluator.WinFreeSpins, evaluator.FreeSpinsMultiplier, controls.LineBet, config.multiplyFreeSpinsByLineBet);
            if (winSpins > 0) Sound.PlayWinFreeSpin(winCoins > 0 || jackpotCoins > 0 ? 1.5f : 0f);
            controls.AddFreeSpins(winSpins);
            playFreeSpins = controls.HasFreeSpins;

            // scatter reward
            if (evaluator.ScatterWin != null && evaluator.ScatterWin.LevelProgress > 0f)
            {
                wallet.AddLevelProgress(evaluator.ScatterWin.LevelProgress);
                Sound.PlayWinCoins();
            }

            // level progress for the winning lines
            wallet.AddLevelProgress(config.winSpinLevelProgress * evaluator.WinLinesCount * controls.LineBet);

            // the player may act while the win is presented
            spinning = false;
            if (!playFreeSpins) SetInputActivity(true);
            Sound.PlayMusic();

            bool showDone = false;
            winPresenter.ShowSequence(true, () => showDone = true);
            while (!showDone) yield return new WaitForSeconds(0.2f);
        }
        #endregion results

        /// <summary>Lets the player touch the controls only between spins.</summary>
        private void SetInputActivity(bool active)
        {
            if (active)
            {
                if (controls.HasFreeSpins) controls.SetControlActivity(false, true); // keep the bet while free spins last
                else controls.SetControlActivity(true, true);
            }
            else
            {
                controls.SetControlActivity(false, controls.Auto); // spin stays pressable to stop auto play
            }
        }
    }
}
