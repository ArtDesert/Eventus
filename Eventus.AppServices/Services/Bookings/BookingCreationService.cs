using Eventus.AppServices.Queues.Bookings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Eventus.AppServices.Services.Bookings;

public class BookingCreationService(
    IBookingTaskQueue queue,
    ILogger<BookingCreationService> logger,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BookingCreationService started.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (queue.TryDequeue(out var task))
                {
                    logger.LogInformation("Booking task: {TaskId} started.", task!.Id);

                    // Simulation of processing...
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    
                    using ( var scope = scopeFactory.CreateScope())
                    {
                        var bookingsService = scope.ServiceProvider.GetRequiredService<IBookingsService>();
                        await bookingsService.ConfirmBookingAsync(task.BookingId, stoppingToken);
                    }
                    
                    logger.LogInformation("Booking task: {TaskId} has been completed successfully.", task.Id);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when booking creation.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }

        logger.LogInformation("BookingCreationService has been stopped successfully.");
    }
}