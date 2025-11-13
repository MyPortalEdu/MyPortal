namespace MyPortal.Common.Constants
{
    public static class RegularExpressions
    {
        public const string ColourCode = @"^#([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$";
        public const string EmailAddress = @"/^([a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6})*$/";

        public const string PostCode =
            @"^([A-Za-z][A-Ha-hJ-Yj-y]?[0-9][A-Za-z0-9]? ?[0-9][A-Za-z]{2}|[Gg][Ii][Rr] ?0[Aa]{2})$";

        public const string NationalInsuranceNumber =
            @"^(?!BG)(?!GB)(?!NK)(?!KN)(?!TN)(?!NT)(?!ZZ)(?:[A-CEGHJ-PR-TW-Z][A-CEGHJ-NPR-TW-Z])(?:\s*\d\s*){6}([A-D]|\s)$";

        public const string AllowedDirectoryNameChars = @"^[A-Za-z0-9 _\\-\\(\\)]+$";

        public const string AllowedDocumentNameChars = @"^[A-Za-z0-9 _\-\.,:]+$";
    }
}