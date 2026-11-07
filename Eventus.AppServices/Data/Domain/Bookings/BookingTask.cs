namespace Eventus.AppServices.Data.Domain.Bookings;

public class BookingTask
{
    public BookingTask(Guid id, Guid bookingId)
    {
        Id = id;
        BookingId = bookingId;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid Id { get; }
    
    public Guid BookingId { get; }
    
    public DateTime CreatedAt { get; }
}