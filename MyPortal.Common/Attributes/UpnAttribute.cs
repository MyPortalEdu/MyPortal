using System.ComponentModel.DataAnnotations;

namespace MyPortal.Common.Attributes
{
    internal class UpnAttribute : ValidationAttribute
    {
        public static bool ValidateUpn(string value)
        {
            value = value.Replace(" ", "");

            if (value.Length != 13)
            {
                return false;
            }

            var chars = value.ToCharArray();

            var checkDigit = GetUpnCheckDigit(chars[new Range(1, 13)]);

            return chars[0] == checkDigit;
        }

        public static char GetUpnCheckDigit(char[] baseUpn)
        {
            if (baseUpn.Length != 12)
            {
                throw new ArgumentException("Please enter the base UPN only", nameof(baseUpn));
            }

            var alpha = new[]
            {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'T', 'U', 'V', 'W', 'X',
                'Y', 'Z'
            };

            var check = 0;

            for (var i = 1; i < baseUpn.Length + 1; i++)
            {
                if (int.TryParse(baseUpn[i - 1].ToString(), out var x))
                {
                    var n = x * (i + 1);
                    check += n;
                }
            }

            var alphaIndex = check % 23;

            return alpha[alphaIndex];
        }

        
        protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
        {
            if (value is string stringValue && ValidateUpn(stringValue))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("UPN is invalid.");
        }
    }
}