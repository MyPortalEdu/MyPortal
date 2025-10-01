namespace MyPortal.Services.Interfaces;

public interface IValidationService
{
    Task ValidateAsync<T>(T model);
}