namespace MyPortal.Common.Exceptions;

public class ForbiddenException(string? message) : Exception(message);