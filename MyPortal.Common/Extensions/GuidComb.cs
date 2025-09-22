namespace MyPortal.Common.Extensions;

public static class GuidComb
{
    // SQL Server’s DATETIME accuracy is ~3.3333333333333335 ms (1/300th).
    private const double SqlServerTickMs = 3.3333333333333335;

    private static readonly DateTime BaseDateUtc =
        new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Generates a COMB GUID optimized for SQL Server clustered indexes.
    /// 10 random bytes + 6 time bytes (2 = days since 1900-01-01, 4 = ms-of-day / 3.333...).
    /// </summary>
    public static Guid Generate()
    {
        var guidArray = Guid.NewGuid().ToByteArray();

        var now = DateTime.UtcNow;
        var days = (now - BaseDateUtc).Days; // fits in 2 bytes until year 2079
        var msecs = (int)(now.TimeOfDay.TotalMilliseconds / SqlServerTickMs);

        var daysArray = BitConverter.GetBytes((short)days);   // 2 bytes
        var msecsArray = BitConverter.GetBytes(msecs);        // 4 bytes

        // SQL Server’s GUID ordering expects these reversed here.
        Array.Reverse(daysArray);
        Array.Reverse(msecsArray);

        // Write into the LAST 6 bytes of the GUID
        // [.... .... .... .... .... dd mm mm mm mm]
        Array.Copy(daysArray, 0, guidArray, guidArray.Length - 6, 2);
        Array.Copy(msecsArray, 0, guidArray, guidArray.Length - 4, 4);

        return new Guid(guidArray);
    }
}