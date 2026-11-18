namespace Eventus.AppServices.Data;

public static class EventsBookingsSemaphore
{
    public static SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);
}