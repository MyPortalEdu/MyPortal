using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyPortal.Services.Interfaces;

namespace MyPortal.Services.Validation;

public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }


    public async Task ValidateAsync<T>(T model)
    {
        var validator = _serviceProvider.GetRequiredService<IValidator<T>>();

        if (validator == null)
        {
            throw new ValidationException($"No validator found for type {typeof(T).Name}");
        }
        
        var result = await validator.ValidateAsync(model);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}