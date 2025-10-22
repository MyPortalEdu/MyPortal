using FluentValidation;
using FluentValidation.Results;
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
        await validator.ValidateAndThrowAsync(model);
    }
    
    public async Task<IList<ValidationFailure>> TryValidateAsync<T>(T model)
    {
        var validator = _serviceProvider.GetService<IValidator<T>>();
        if (validator == null) return new List<ValidationFailure>();
    
        var result = await validator.ValidateAsync(model);
        return result.IsValid ? [] : result.Errors;
    }
}