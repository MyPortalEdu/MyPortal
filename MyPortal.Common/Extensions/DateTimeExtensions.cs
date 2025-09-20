using MyPortal.Common.Enums;
using MyPortal.Common.Models;

namespace MyPortal.Common.Extensions
{
    internal static class DateTimeExtensions
    {
        internal static IEnumerable<DateTime> GetAllInstancesUntil(this DateTime startDate, DateTime endDate,
            DateTimeDivision division = DateTimeDivision.Day, int incrementBy = 1)
        {
            var instances = new List<DateTime>();

            Func<DateTime, DateTime> increment;

            switch (division)
            {
                case DateTimeDivision.Year:
                    increment = time => time.AddYears(incrementBy);
                    break;
                case DateTimeDivision.Month:
                    increment = time => time.AddMonths(incrementBy);
                    break;
                case DateTimeDivision.Day:
                    increment = time => time.AddDays(incrementBy);
                    break;
                case DateTimeDivision.Hour:
                    increment = time => time.AddHours(incrementBy);
                    break;
                case DateTimeDivision.Minute:
                    increment = time => time.AddMinutes(incrementBy);
                    break;
                case DateTimeDivision.Second:
                    increment = time => time.AddSeconds(incrementBy);
                    break;
                case DateTimeDivision.Millisecond:
                    increment = time => time.AddMilliseconds(incrementBy);
                    break;
                default:
                    increment = time => time.AddTicks(incrementBy);
                    break;
            }

            for (var dt = startDate; dt <= endDate; dt = increment(dt))
            {
                instances.Add(dt);
            }

            return instances;
        }

        
        internal static DateTime GetDayOfWeek(this DateTime dateTime, DayOfWeek dayOfWeek,
            SundayPosition sundayPosition = SundayPosition.WeekEnd)
        {
            var currentDayOfWeek = (int)dateTime.DayOfWeek;

            var target = sundayPosition == SundayPosition.WeekEnd && dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;

            return dateTime.AddDays(target - currentDayOfWeek);
        }

        internal static DateTime? GetNextOccurrence(this DateTime dateTime, EventFrequency frequency)
        {
            switch (frequency)
            {
                case EventFrequency.Daily:
                    return dateTime.AddDays(1);
                case EventFrequency.Weekly:
                    return dateTime.AddDays(7);
                case EventFrequency.BiWeekly:
                    return dateTime.AddDays(14);
                case EventFrequency.Monthly:
                    return dateTime.AddMonths(1);
                case EventFrequency.BiMonthly:
                    return dateTime.AddMonths(2);
                case EventFrequency.Annually:
                    return dateTime.AddYears(1);
                case EventFrequency.BiAnnually:
                    return dateTime.AddYears(2);
                default:
                    return null;
            }
        }

        internal static DateTime? GetNextDailyOccurrence(this DateTime dateTime, WeeklyPattern weeklyPattern)
        {
            DateTime currentDate = dateTime.AddDays(1);

            while (currentDate <= weeklyPattern.EndDate)
            {
                if (weeklyPattern.Days.Contains(currentDate.DayOfWeek))
                {
                    return currentDate;
                }

                currentDate = currentDate.AddDays(1);
            }

            return null;
        }

        internal static bool IsWeekend(this DateTime dateTime)
        {
            return dateTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        }

        internal static DateTime GetEndOfDay(this DateTime dateTime)
        {
            return dateTime.Date.AddTicks(TimeSpan.TicksPerDay - 1);
        }
    }
}