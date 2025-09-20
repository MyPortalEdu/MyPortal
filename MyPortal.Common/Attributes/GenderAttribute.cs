using System.ComponentModel.DataAnnotations;
using MyPortal.Logic.Constants;

namespace MyPortal.Common.Attributes
{
    internal class GenderAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string stringValue)
            {
                var validValues = new[] { Genders.Male, Genders.Female, Genders.Other, Genders.Unknown };

                if (validValues.Contains(stringValue))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult("Gender is invalid.");
        }
    }
}