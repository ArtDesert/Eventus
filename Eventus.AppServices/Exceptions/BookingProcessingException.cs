namespace Eventus.AppServices.Exceptions;

public class BookingProcessingException : Exception
{
    public BookingProcessingException() : base()
    {
    }

    public BookingProcessingException(string message) : base(message)
    {
    }

    public BookingProcessingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}