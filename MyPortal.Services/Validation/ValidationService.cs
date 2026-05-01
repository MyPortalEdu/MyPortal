using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyPortal.Services.Interfaces;

namespace MyPortal.Services.Validation;

public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(IServiceProvider serviceProvider, ILogger<ValidationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ValidateAsync<T>(T model)
    {
        var validator = _serviceProvider.GetRequiredService<IValidator<T>>();
        await validator.ValidateAndThrowAsync(model);
    }

    public async Task<IList<ValidationFailure>> TryValidateAsync<T>(T model)
    {
        var validator = _serviceProvider.GetService<IValidator<T>>();
        if (validator == null)
        {
            // Treating "no validator registered" as success is the documented contract for this
            // method, but it's also the kind of refactor mistake that masks real bugs. Log a
            // warning so the gap surfaces in observability instead of disappearing silently.
            _logger.LogWarning("No validator registered for {ModelType}; treating as valid.", typeof(T).FullName);
            return new List<ValidationFailure>();
        }

        var result = await validator.ValidateAsync(model);
        return result.IsValid ? [] : result.Errors;
    }
}