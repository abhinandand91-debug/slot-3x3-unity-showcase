using System;

namespace Slot3x3
{
    /// <summary>What the daily reward looks like at a given moment.</summary>
    public readonly struct DailyRewardStatus
    {
        public readonly bool Available;
        /// <summary>Index of the reward (0-based) that is available now or will be next.</summary>
        public readonly int Day;
        /// <summary>Time until the reward becomes available (zero when it already is).</summary>
        public readonly TimeSpan Remaining;

        public DailyRewardStatus(bool available, int day, TimeSpan remaining)
        {
            Available = available;
            Day = day;
            Remaining = remaining;
        }
    }

    /// <summary>
    /// The rules of the daily reward, kept free of Unity so they are easy to test: one reward every interval,
    /// rewards advance day by day, and letting a whole extra interval pass restarts the streak at day 1.
    /// </summary>
    public class DailyRewardSchedule
    {
        private readonly int days;
        private readonly TimeSpan interval;

        public DailyRewardSchedule(int days, TimeSpan interval)
        {
            this.days = Math.Max(1, days);
            this.interval = interval;
        }

        public DailyRewardStatus Evaluate(DateTime? lastClaimUtc, int nextDay, DateTime nowUtc)
        {
            if (!lastClaimUtc.HasValue) return new DailyRewardStatus(true, 0, TimeSpan.Zero);

            TimeSpan elapsed = nowUtc - lastClaimUtc.Value;
            if (elapsed < TimeSpan.Zero) elapsed = TimeSpan.Zero; // clock was set back

            int day = ((nextDay % days) + days) % days;
            if (elapsed >= interval + interval) return new DailyRewardStatus(true, 0, TimeSpan.Zero); // streak lost
            if (elapsed >= interval) return new DailyRewardStatus(true, day, TimeSpan.Zero);
            return new DailyRewardStatus(false, day, interval - elapsed);
        }

        public int DayAfter(int day)
        {
            return (day + 1) % days;
        }
    }
}
