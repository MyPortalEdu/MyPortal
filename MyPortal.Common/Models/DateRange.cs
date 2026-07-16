using MyPortal.Common.Extensions;

namespace MyPortal.Common.Models;

/// <summary>Represents an inclusive start / exclusive end date range.</summary>
public class DateRange
{
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }

    public TimeSpan Duration => End - Start;

    public static DateRange CurrentWeek
    {
        get
        {
            var monday = DateTime.Today.GetDayOfWeek(DayOfWeek.Monday);
            return new DateRange(monday, monday.AddDays(6));
        }
    }

    public DateRange(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("End must not be before Start.", nameof(end));

        Start = start;
        End = end;
    }

    public bool Overlaps(DateRange other, bool includeAdjacent = false) =>
        (Start < other.End && End > other.Start) ||
        (includeAdjacent && IsAdjacentTo(other));

    public bool IsAdjacentTo(DateRange other) =>
        End == other.Start || Start == other.End;

    /// <summary>Expands this range to also cover <paramref name="other"/>.</summary>
    public void Merge(DateRange other)
    {
        if (other.Start < Start) Start = other.Start;
        if (other.End > End) End = other.End;
    }

    /// <summary>Shifts the range so it begins at <paramref name="newStart"/>.</summary>
    public void MoveToStart(DateTime newStart) => Move(newStart - Start);

    /// <summary>Shifts both endpoints by <paramref name="offset"/>.</summary>
    public void Move(TimeSpan offset)
    {
        Start = Start.Add(offset);
        End = End.Add(offset);
    }

    /// <summary>Stretches the end of the range by <paramref name="amount"/>.</summary>
    public void Extend(TimeSpan amount)
    {
        if (amount < TimeSpan.Zero)
            throw new ArgumentException("Cannot extend by a negative amount.", nameof(amount));

        End = End.Add(amount);
    }

    /// <summary>
    /// Attempts to coalesce this range with <paramref name="other"/>.
    /// Returns <c>false</c> when the two ranges are not adjacent.
    /// </summary>
    public bool TryCoalesce(DateRange other, out DateRange? coalesced)
    {
        if (!IsAdjacentTo(other))
        {
            coalesced = null;
            return false;
        }

        coalesced = End == other.Start
            ? new DateRange(Start, other.End)
            : new DateRange(other.Start, End);

        return true;
    }

    public IEnumerable<DateTime> GetAllDates() => Start.GetAllInstancesUntil(End);

    public (DateTime Start, DateTime End) ToTuple() => (Start, End);

    public override string ToString() => $"{Start:d} – {End:d}";
}