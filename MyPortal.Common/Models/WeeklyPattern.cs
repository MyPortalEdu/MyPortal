namespace MyPortal.Common.Models;

public class WeeklyPattern(DayOfWeek[] days, DateTime endDate)
{
    public DayOfWeek[] Days { get; set; } = days;
    public DateTime EndDate { get; set; } = endDate;
}