namespace Eventus.AppServices.Options;

public class BookingProcessingOptions
{
    public int BookingProcessingSimulationMilliseconds { get; init; } = 2000;

    public int WorkerCount { get; init; } = 4;
}