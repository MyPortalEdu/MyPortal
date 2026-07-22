namespace MyPortal.Common.Identifiers;

public static class Upn
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRTUVWXYZ";

    public const int Length = 13;
    
    public static char CheckLetter(ReadOnlySpan<char> significant)
    {
        if (significant.Length != 12)
        {
            throw new ArgumentException("A UPN has 12 significant characters.", nameof(significant));
        }

        var sum = 0;

        for (var i = 0; i < 12; i++)
        {
            sum += (significant[i] - '0') * (i + 2);
        }

        return Alphabet[sum % 23];
    }
    
    public static string Compose(int laCode, int establishmentNumber, int allocationYear, int serial)
    {
        if (serial is < 0 or > 999)
        {
            throw new ArgumentOutOfRangeException(nameof(serial), serial, "UPN serial must be 0–999.");
        }

        var significant = $"{laCode:D3}{establishmentNumber:D4}{allocationYear % 100:D2}{serial:D3}";

        return CheckLetter(significant) + significant;
    }
    
    public static bool IsValid(string? upn)
    {
        if (string.IsNullOrEmpty(upn) || upn.Length != Length)
        {
            return false;
        }

        var u = upn.ToUpperInvariant();

        if (Alphabet.IndexOf(u[0]) < 0)
        {
            return false;
        }

        for (var i = 1; i <= 11; i++)
        {
            if (!char.IsAsciiDigit(u[i]))
            {
                return false;
            }
        }

        var last = u[12];

        if (char.IsAsciiDigit(last))
        {
            return CheckLetter(u.AsSpan(1, 12)) == u[0];
        }

        return Alphabet.IndexOf(last) >= 0;
    }
}
