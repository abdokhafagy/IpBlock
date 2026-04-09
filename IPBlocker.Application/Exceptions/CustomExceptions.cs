namespace IPBlocker.Application.Exceptions;

public class DuplicateException : Exception
{
    public DuplicateException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ExternalApiException : Exception
{
    public ExternalApiException(string message) : base(message) { }
    public ExternalApiException(string message, Exception innerException) : base(message, innerException) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
