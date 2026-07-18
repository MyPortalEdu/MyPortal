using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyPortal.Services.Interfaces;

namespace MyPortal.Services.Validation;

public class ValidationService(IServiceProvider serviceProvider, ILogger<ValidationService> logger)
    : IValidationService
{
    public async Task ValidateAsync<T>(T model)
    {
        var validator = serviceProvider.GetRequiredService<IValidator<T>>();
        await validator.ValidateAndThrowAsync(model);
    }

    public async Task<IList<ValidationFailure>> TryValidateAsync<T>(T model)
    {
        var validator = serviceProvider.GetService<IValidator<T>>();
        if (validator == null)
        {
            // Treating "no validator registered" as success is the documented contract for this
            // method, but it's also the kind of refactor mistake that masks real bugs. Log a
            // warning so the gap surfaces in observability instead of disappearing silently.
            logger.LogWarning("No validator registered for {ModelType}; treating as valid.", typeof(T).FullName);
            return new List<ValidationFailure>();
        }

        var result = await validator.ValidateAsync(model);
        return result.IsValid ? [] : result.Errors;
    }
}