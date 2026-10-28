using System;
using System.Globalization;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// Gives the player a coin reward once a day. It remembers when the last reward was taken and which day of the
    /// streak comes next, opens the reward window when a reward is waiting, and shows a badge on the header button.
    /// </summary>
    public class DailyRewardController : MonoBehaviour
    {
        private const string LastClaimKey = "slot3x3_daily_last";
        private const string NextDayKey = "slot3x3_daily_next";

        [SerializeField] private GameObject badge;
        [SerializeField] private DailyRewardPopup popup;
        [SerializeField, Tooltip("Seconds after the game starts before a waiting reward pops up")]
        private float popupDelay = 1.5f;

        private SlotConfig config;
        private PlayerWallet wallet;
        private DailyRewardSchedule schedule;
        private DateTime? lastClaim;
        private int nextDay;
        private float refreshTimer;

        /// <summary>Replaceable clock, so tests and developers can jump through time.</summary>
        public Func<DateTime> UtcNow = () => DateTime.UtcNow;

        public DailyRewardStatus Status { get; private set; }
        public int[] Rewards { get { return config.dailyRewards; } }

        public void Setup(GameObject badgeObject, DailyRewardPopup rewardPopup)
        {
            badge = badgeObject;
            popup = rewardPopup;
        }

        public void Init(SlotConfig slotConfig, PlayerWallet playerWallet)
        {
            config = slotConfig;
            wallet = playerWallet;
            schedule = new DailyRewardSchedule(config.dailyRewards.Length, TimeSpan.FromHours(config.dailyRewardIntervalHours));

            Load();
            Refresh();
            popup.Init(this);
            if (Status.Available) Invoke(nameof(OpenPopup), popupDelay);
        }

        private void OpenPopup()
        {
            popup.Open();
        }

        private void Update()
        {
            if (schedule == null) return;
            refreshTimer += Time.unscaledDeltaTime;
            if (refreshTimer < 1f) return;
            refreshTimer = 0f;
            Refresh();
        }

        public void Refresh()
        {
            Status = schedule.Evaluate(lastClaim, nextDay, UtcNow());
            badge.SetActive(Status.Available);
            popup.Refresh();
        }

        /// <summary>Takes the waiting reward. Returns the coins granted, or 0 if there is none yet.</summary>
        public int Claim()
        {
            if (!Status.Available) return 0;

            int coins = config.dailyRewards[Status.Day];
            wallet.AddCoins(coins);

            lastClaim = UtcNow();
            nextDay = schedule.DayAfter(Status.Day);
            Save();
            Refresh();
            return coins;
        }

        /// <summary>Developer helper: forget the streak so the first reward is available again.</summary>
        public void ResetData()
        {
            lastClaim = null;
            nextDay = 0;
            GameServices.Save.Delete(LastClaimKey);
            GameServices.Save.Delete(NextDayKey);
            Refresh();
        }

        #region saving
        private void Load()
        {
            string saved = GameServices.Save.GetString(LastClaimKey, "");
            lastClaim = long.TryParse(saved, NumberStyles.Integer, CultureInfo.InvariantCulture, out long ticks)
                ? new DateTime(ticks, DateTimeKind.Utc)
                : (DateTime?)null;
            nextDay = GameServices.Save.GetInt(NextDayKey, 0);
        }

        private void Save()
        {
            GameServices.Save.SetString(LastClaimKey, lastClaim.Value.Ticks.ToString(CultureInfo.InvariantCulture));
            GameServices.Save.SetInt(NextDayKey, nextDay);
        }
        #endregion saving
    }
}
