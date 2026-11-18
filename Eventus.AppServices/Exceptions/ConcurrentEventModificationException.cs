namespace Eventus.AppServices.Exceptions;

public class ConcurrentEventModificationException : Exception
{
    public ConcurrentEventModificationException() : base()
    {
    }

    public ConcurrentEventModificationException(string message) : base(message)
    {
    }

    public ConcurrentEventModificationException(string message, Exception innerException) : base(message,
        innerException)
    {
    }
}