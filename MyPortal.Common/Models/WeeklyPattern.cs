namespace MyPortal.Common.Models;

public class WeeklyPattern
{
    public WeeklyPattern(DayOfWeek[] days, DateTime endDate)
    {
        Days = days;
        EndDate = endDate;
    }

    public DayOfWeek[] Days { get; set; }
    public DateTime EndDate { get; set; }
}