namespace MyPortal.Common.Constants
{
    public static class Genders
    {
        public const string Male = "M";
        public const string Female = "F";
        public const string Other = "X";
        public const string Unknown = "U";

        private static readonly Dictionary<string, string> GenderLabels = new Dictionary<string, string>
        {
            { Male, "Male" },
            { Female, "Female" },
            { Other, "Other" },
            { Unknown, "Unknown" }
        };

        public static string? GetGenderLabel(string gender)
        {
            return GenderLabels.GetValueOrDefault(gender);
        }
    }
}