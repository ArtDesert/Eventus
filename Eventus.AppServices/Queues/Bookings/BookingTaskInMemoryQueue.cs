using System.Collections.Concurrent;
using Eventus.AppServices.Data.Domain.Bookings;

namespace Eventus.AppServices.Queues.Bookings;

public class BookingTaskInMemoryQueue : IBookingTaskQueue
{
    private readonly ConcurrentQueue<BookingTask> _queue = new();

    public void Enqueue(BookingTask task)
    {
        _queue.Enqueue(task);
    }

    public bool TryDequeue(out BookingTask? task)
    {
        return _queue.TryDequeue(out task);
    }
}