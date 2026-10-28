using System;
using UnityEngine;
using UnityEngine.UI;

namespace Slot3x3
{
    /// <summary>
    /// The daily reward window: one plate per day of the streak, the current day highlighted, and a collect button
    /// that turns into a countdown while the next reward is not ready.
    /// </summary>
    public class DailyRewardPopup : PopupWindow
    {
        private static readonly Color PlateNormal = new Color(0.20f, 0.13f, 0.34f, 1f);
        private static readonly Color PlateCurrent = new Color(0.62f, 0.34f, 0.98f, 1f);
        private static readonly Color PlateTaken = new Color(0.10f, 0.09f, 0.13f, 1f);

        [SerializeField] private Image[] plates;
        [SerializeField] private Text[] dayTexts;
        [SerializeField] private Text[] amountTexts;
        [SerializeField] private Button collectButton;
        [SerializeField] private Text collectLabel;

        private DailyRewardController controller;

        public void SetupDays(Image[] dayPlates, Text[] days, Text[] amounts, Button collect, Text label)
        {
            plates = dayPlates;
            dayTexts = days;
            amountTexts = amounts;
            collectButton = collect;
            collectLabel = label;
        }

        public void Init(DailyRewardController rewards)
        {
            controller = rewards;
            for (int i = 0; i < amountTexts.Length; i++)
                amountTexts[i].text = NumberFormat.Coins(controller.Rewards[i]);

            collectButton.onClick.AddListener(() =>
            {
                int coins = controller.Claim();
                if (coins > 0) GameServices.Sound.PlayWinCoins();
            });
            Refresh();
        }

        public override void Open()
        {
            Refresh();
            base.Open();
        }

        public void Refresh()
        {
            if (controller == null) return;
            DailyRewardStatus status = controller.Status;

            for (int i = 0; i < plates.Length; i++)
            {
                bool current = i == status.Day;
                bool taken = i < status.Day;
                plates[i].color = current ? PlateCurrent : (taken ? PlateTaken : PlateNormal);
                dayTexts[i].text = current && status.Available ? "GET" : "DAY " + (i + 1);
            }

            collectButton.interactable = status.Available;
            collectLabel.text = status.Available ? "COLLECT" : Countdown(status.Remaining);
        }

        private static string Countdown(TimeSpan remaining)
        {
            return string.Format("{0:00}:{1:00}:{2:00}", (int)remaining.TotalHours, remaining.Minutes, remaining.Seconds);
        }
    }
}
