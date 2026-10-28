using System;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// The player's coins and level progress. Coins and level are saved through <see cref="ISaveStore"/>;
    /// everything else in the game listens to the events to refresh its display.
    /// </summary>
    public class PlayerWallet : MonoBehaviour
    {
        private const string CoinsKey = "slot3x3_coins";
        private const string LevelKey = "slot3x3_level";
        private const string ProgressKey = "slot3x3_level_progress";

        [SerializeField] private int defaultCoins = 500;
        [SerializeField] private bool saveData = true;

        public static PlayerWallet Instance { get; private set; }

        public int Coins { get; private set; }
        public int WinCoins { get; private set; }
        public int Level { get; private set; }
        /// <summary>Progress to the next level, 0..100.</summary>
        public float LevelProgress { get; private set; }

        public event Action<int> CoinsChanged;
        public event Action<int> WinCoinsChanged;
        public event Action<int> LevelChanged;
        public event Action<float> LevelProgressChanged;

        // where the values are kept; a throwaway memory store when saving is off
        private ISaveStore store;

        public void Configure(int startCoins)
        {
            defaultCoins = startCoins;
        }

        private void Awake()
        {
            Initialize();
        }

        /// <summary>Registers the singleton and loads the saved values (called from Awake; public so tests can run it in edit mode).</summary>
        public void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Load();
        }

        private void Load()
        {
            store = saveData ? GameServices.Save : new MemorySaveStore();
            Coins = store.GetInt(CoinsKey, defaultCoins);
            Level = store.GetInt(LevelKey, 0);
            LevelProgress = store.GetFloat(ProgressKey, 0f);
        }

        /// <summary>Pushes the loaded values to listeners that subscribed after Awake.</summary>
        public void Announce()
        {
            CoinsChanged?.Invoke(Coins);
            LevelChanged?.Invoke(Level);
            LevelProgressChanged?.Invoke(LevelProgress);
        }

        #region coins
        public bool HasMoneyForBet(int totalBet)
        {
            return totalBet <= Coins;
        }

        public void AddCoins(int count)
        {
            SetCoins(Coins + count);
        }

        private void SetCoins(int count)
        {
            count = Mathf.Max(0, count);
            if (count == Coins) return;
            Coins = count;
            store.SetInt(CoinsKey, Coins);
            CoinsChanged?.Invoke(Coins);
        }

        /// <summary>Sets the amount shown as "win" for the current spin (it is already credited to the balance).</summary>
        public void SetWinCoins(int count)
        {
            count = Mathf.Max(0, count);
            if (count == WinCoins) return;
            WinCoins = count;
            WinCoinsChanged?.Invoke(WinCoins);
        }
        #endregion coins

        #region level
        /// <summary>Adds progress in percent of a level; every 100 raises the level.</summary>
        public void AddLevelProgress(float percent)
        {
            float progress = Mathf.Max(0f, LevelProgress + percent);
            if (progress >= 100f)
            {
                SetLevel(Level + (int)(progress / 100f));
                progress = 0f;
            }

            bool changed = !Mathf.Approximately(progress, LevelProgress);
            LevelProgress = progress;
            store.SetFloat(ProgressKey, LevelProgress);
            if (changed) LevelProgressChanged?.Invoke(LevelProgress);
        }

        private void SetLevel(int level)
        {
            Level = Mathf.Max(0, level);
            store.SetInt(LevelKey, Level);
            LevelChanged?.Invoke(Level);
        }
        #endregion level
    }
}
