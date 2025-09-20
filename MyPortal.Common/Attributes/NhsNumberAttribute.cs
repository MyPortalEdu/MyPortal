using System.ComponentModel.DataAnnotations;

namespace MyPortal.Common.Attributes
{
    internal class NhsNumberAttribute : ValidationAttribute
    {
        public static bool ValidateNhsNumber(string value)
        {
            value = value.Replace(" ", "");

            if (value.Length != 10)
            {
                return false;
            }

            if (!int.TryParse(value[9].ToString(), out var checkDigit))
            {
                return false;
            }

            var result = 0;

            for (var i = 0; i < 9; i++)
            {
                if (int.TryParse(value[i].ToString(), out var digitValue))
                {
                    result += digitValue * (10 - i);
                }
                else
                {
                    return false;
                }
            }

            var validationResult = 11 - result % 11;

            if (validationResult == 10) return false;

            return validationResult == 11 ? checkDigit == 0 : checkDigit == validationResult;
        }
        
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string stringValue && ValidateNhsNumber(stringValue))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("NHS number is invalid.");
        }
    }
}