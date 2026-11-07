using Eventus.AppServices.Data.Domain.Bookings;

namespace Eventus.AppServices.Queues.Bookings;

public interface IBookingTaskQueue
{
    void Enqueue(BookingTask task);
    bool TryDequeue(out BookingTask? task);
}