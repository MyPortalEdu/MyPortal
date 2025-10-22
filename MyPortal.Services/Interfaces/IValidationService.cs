using FluentValidation.Results;

namespace MyPortal.Services.Interfaces;

public interface IValidationService
{
    Task ValidateAsync<T>(T model);

    Task<IList<ValidationFailure>> TryValidateAsync<T>(T model);
}