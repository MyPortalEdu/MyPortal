namespace MyPortal.Common.Exceptions;

public class SystemEntityException : Exception
{
    public SystemEntityException(string? message) : base(message)
    {
    }
}