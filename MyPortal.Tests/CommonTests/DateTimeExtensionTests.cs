using MyPortal.Common.Enums;
using MyPortal.Common.Models;
using MyPortal.Common.Extensions;

namespace MyPortal.Tests.CommonTests
{
    [TestFixture]
    public class DateTimeExtensionsTests
    {
        [Test]
        public void GetAllInstancesUntil_ShouldReturnEveryDay_WhenUsingDefaultDivision()
        {
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 3);

            var result = start.GetAllInstancesUntil(end).ToList();

            Assert.That(result, Is.EqualTo(new List<DateTime>
            {
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2),
                new DateTime(2026, 1, 3)
            }));
        }

        [Test]
        public void GetAllInstancesUntil_ShouldRespectMonthDivision()
        {
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 3, 1);

            var result = start.GetAllInstancesUntil(
                end,
                DateTimeDivision.Month).ToList();

            Assert.That(result, Is.EqualTo(new List<DateTime>
            {
                new DateTime(2026, 1, 1),
                new DateTime(2026, 2, 1),
                new DateTime(2026, 3, 1)
            }));
        }

        [Test]
        public void GetDayOfWeek_ShouldReturnCorrectWeekday()
        {
            var date = new DateTime(2026, 4, 27); // Monday

            var result = date.GetDayOfWeek(DayOfWeek.Friday);

            Assert.That(result, Is.EqualTo(new DateTime(2026, 5, 1)));
        }

        [Test]
        public void GetDayOfWeek_ShouldTreatSundayAsWeekEnd_WhenConfigured()
        {
            var date = new DateTime(2026, 4, 27); // Monday

            var result = date.GetDayOfWeek(
                DayOfWeek.Sunday,
                SundayPosition.WeekEnd);

            Assert.That(result, Is.EqualTo(new DateTime(2026, 5, 3)));
        }

        [Test]
        public void GetNextOccurrence_ShouldReturnNextWeeklyOccurrence()
        {
            var date = new DateTime(2026, 4, 27);

            var result = date.GetNextOccurrence(EventFrequency.Weekly);

            Assert.That(result, Is.EqualTo(new DateTime(2026, 5, 4)));
        }

        [Test]
        public void GetNextOccurrence_ShouldReturnNull_ForUnsupportedFrequency()
        {
            var date = new DateTime(2026, 4, 27);

            var result = date.GetNextOccurrence((EventFrequency)999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetNextDailyOccurrence_ShouldReturnNextMatchingDay()
        {
            var start = new DateTime(2026, 4, 27); // Monday

            var days = new List<DayOfWeek>
            {
                DayOfWeek.Wednesday,
                DayOfWeek.Friday
            }.ToArray();

            var pattern = new WeeklyPattern(
                days,
                new DateTime(2026, 5, 10)
            );

            var result = start.GetNextDailyOccurrence(pattern);

            Assert.That(result, Is.EqualTo(new DateTime(2026, 4, 29)));
        }

        [Test]
        public void GetNextDailyOccurrence_ShouldReturnNull_WhenNoMatchingDayExists()
        {
            var start = new DateTime(2026, 4, 27); // Monday

            var days = new List<DayOfWeek>
            {
                DayOfWeek.Sunday
            }.ToArray();

            var pattern = new WeeklyPattern(days, new DateTime(2026, 4, 28));

            var result = start.GetNextDailyOccurrence(pattern);

            Assert.That(result, Is.Null);
        }

        [TestCase(2026, 4, 25, true)]  // Saturday
        [TestCase(2026, 4, 26, true)]  // Sunday
        [TestCase(2026, 4, 27, false)] // Monday
        public void IsWeekend_ShouldReturnExpectedResult(
            int year,
            int month,
            int day,
            bool expected)
        {
            var date = new DateTime(year, month, day);

            var result = date.IsWeekend();

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void GetEndOfDay_ShouldReturnLastTickOfTheDay()
        {
            var date = new DateTime(2026, 4, 27, 10, 30, 0);

            var result = date.GetEndOfDay();

            var expected = new DateTime(2026, 4, 27)
                .AddDays(1)
                .AddTicks(-1);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}