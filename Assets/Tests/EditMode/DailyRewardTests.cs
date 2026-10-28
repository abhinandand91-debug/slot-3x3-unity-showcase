using System;
using NUnit.Framework;

namespace Slot3x3.Tests
{
    public class DailyRewardTests
    {
        private static readonly DateTime T0 = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        private DailyRewardSchedule schedule;

        [SetUp]
        public void Create()
        {
            schedule = new DailyRewardSchedule(7, TimeSpan.FromHours(24));
        }

        [Test]
        public void FirstRewardIsAvailableImmediately()
        {
            DailyRewardStatus s = schedule.Evaluate(null, 0, T0);
            Assert.IsTrue(s.Available);
            Assert.AreEqual(0, s.Day);
        }

        [Test]
        public void RewardIsLockedWithACountdownRightAfterClaiming()
        {
            DailyRewardStatus s = schedule.Evaluate(T0, 1, T0.AddHours(6));
            Assert.IsFalse(s.Available);
            Assert.AreEqual(1, s.Day);
            Assert.AreEqual(TimeSpan.FromHours(18), s.Remaining);
        }

        [Test]
        public void NextRewardUnlocksAfterTheInterval()
        {
            DailyRewardStatus s = schedule.Evaluate(T0, 3, T0.AddHours(24));
            Assert.IsTrue(s.Available);
            Assert.AreEqual(3, s.Day);
        }

        [Test]
        public void MissingAWholeExtraIntervalRestartsTheStreak()
        {
            DailyRewardStatus s = schedule.Evaluate(T0, 5, T0.AddHours(48));
            Assert.IsTrue(s.Available);
            Assert.AreEqual(0, s.Day);
        }

        [Test]
        public void TheStreakWrapsAfterTheLastDay()
        {
            Assert.AreEqual(0, schedule.DayAfter(6));
            Assert.AreEqual(4, schedule.DayAfter(3));
        }

        [Test]
        public void AClockSetBackNeverGivesAnEarlyReward()
        {
            DailyRewardStatus s = schedule.Evaluate(T0, 1, T0.AddHours(-5));
            Assert.IsFalse(s.Available);
            Assert.AreEqual(TimeSpan.FromHours(24), s.Remaining);
        }
    }
}
