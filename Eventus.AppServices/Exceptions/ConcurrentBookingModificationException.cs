namespace Eventus.AppServices.Exceptions;

public class ConcurrentBookingModificationException : Exception
{
    public ConcurrentBookingModificationException() : base()
    {
    }

    public ConcurrentBookingModificationException(string message) : base(message)
    {
    }

    public ConcurrentBookingModificationException(string message, Exception innerException) : base(message,
        innerException)
    {
    }
}