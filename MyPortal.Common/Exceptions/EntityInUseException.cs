namespace MyPortal.Common.Exceptions;

/// Thrown when a delete or destructive update is rejected because another row
/// still references the target. ExceptionMiddleware maps this to 409 Conflict.
public class EntityInUseException(string? message) : Exception(message);
