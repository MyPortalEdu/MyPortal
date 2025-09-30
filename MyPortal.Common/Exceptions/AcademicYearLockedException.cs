namespace MyPortal.Common.Exceptions;

public class AcademicYearLockedException : Exception
{
    public AcademicYearLockedException(string? message) : base(message)
    {
        
    }
}