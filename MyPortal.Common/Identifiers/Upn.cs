namespace MyPortal.Common.Identifiers;

/// <summary>
/// DfE Unique Pupil Number (UPN) format and check-letter algorithm (UPN Guide v1.2). A UPN is 13
/// characters: a leading check letter followed by 12 significant characters —
/// LA code (3) + establishment number (4) + allocation year (2) + serial (3). The check letter is
/// the running weighted sum of the 12 significant characters (weights 2..13 by position), taken
/// mod 23 and mapped through a 23-letter alphabet that omits I, O and S.
///
/// Generation here produces PERMANENT UPNs (all 12 significant characters numeric). A temporary UPN
/// carries an alphabetic serial in the final position; <see cref="IsValid"/> accepts a well-formed
/// temporary UPN but does not recompute its check letter (that variant is not generated here).
/// </summary>
public static class Upn
{
    // 23-letter alphabet, index == remainder (mod 23) -> check letter. Omits I, O, S.
    private const string Alphabet = "ABCDEFGHJKLMNPQRTUVWXYZ";

    public const int Length = 13;

    /// <summary>
    /// The check letter for a permanent UPN's 12 significant characters (all digits). Weight of the
    /// character at index <c>i</c> is <c>i + 2</c> (positions 2..13 of the full UPN).
    /// </summary>
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

    /// <summary>
    /// Compose a permanent UPN from its parts. <paramref name="allocationYear"/> may be a full year
    /// (its last two digits are used). Throws if <paramref name="serial"/> is outside 0–999.
    /// </summary>
    public static string Compose(int laCode, int establishmentNumber, int allocationYear, int serial)
    {
        if (serial is < 0 or > 999)
        {
            throw new ArgumentOutOfRangeException(nameof(serial), serial, "UPN serial must be 0–999.");
        }

        var significant = $"{laCode:D3}{establishmentNumber:D4}{allocationYear % 100:D2}{serial:D3}";

        return CheckLetter(significant) + significant;
    }

    /// <summary>
    /// True if <paramref name="upn"/> is a structurally valid UPN: 13 chars, a valid leading check
    /// letter, 11 numeric characters in positions 2–12, and a final position that is either a digit
    /// (permanent — the check letter is verified) or an alphabet letter (temporary — accepted as
    /// well-formed; its check letter is not recomputed here).
    /// </summary>
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
