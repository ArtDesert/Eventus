using System.Diagnostics;
using Eventus.AppServices.Data.Enums.Bookings;
using Eventus.AppServices.Options;
using Eventus.AppServices.Queues.Bookings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eventus.AppServices.Services.Bookings;

public class BookingConfirmationService(
    IBookingTaskQueue queue,
    ILogger<BookingConfirmationService> logger,
    IServiceScopeFactory scopeFactory,
    IOptions<BookingProcessingOptions> bookingProcessingOptions)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BookingConfirmationService started.");

        var workers = Enumerable.Range(0, bookingProcessingOptions.Value.WorkerCount)
            .Select(_ => WorkerLoopAsync(stoppingToken));

        await Task.WhenAll(workers);

        logger.LogInformation("BookingConfirmationService has been stopped successfully.");
    }

    private async Task WorkerLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!queue.TryDequeue(out var task))
            {
                await Task.Delay(100, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
            else
            {
                using var scope = scopeFactory.CreateScope();
                var bookingsService = scope.ServiceProvider.GetRequiredService<IBookingsService>();
                try
                {
                    // Simulation of processing...
                    await Task.Delay(
                        TimeSpan.FromMilliseconds(
                            bookingProcessingOptions.Value.BookingProcessingSimulationMilliseconds), stoppingToken);
                    var bookingResult = await bookingsService.ConfirmBookingAsync(task.BookingId, stoppingToken);

                    switch (bookingResult)
                    {
                        case BookingResult.Confirmed:
                            {
                                logger.LogInformation("Booking \'{BookingId}\' has been confirmed.",
                                    task.BookingId);
                                break;
                            }
                        case BookingResult.Rejected:
                            {
                                logger.LogWarning("Booking \'{BookingId}\' has been rejected.",
                                    task.BookingId);
                                break;
                            }
                        case BookingResult.AlreadyProcessed:
                            {
                                logger.LogInformation("Current booking \'{BookingId}\' has already been processed.",
                                    task.BookingId);
                                break;
                            }
                        default:
                            throw new UnreachableException(
                                $"Booking result {bookingResult} is incorrect.");
                    }

                    logger.LogInformation("Booking task \'{TaskId}\' has been processed successfully.", task.Id);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error when booking confirmation {BookingId}. Rejecting", task.BookingId);
                    try
                    {
                        if (await bookingsService.RejectBookingAsync(task.BookingId, CancellationToken.None))
                        {
                            logger.LogWarning("Booking {BookingId} rejected, seat released.", task.BookingId);
                        }
                    }
                    catch (Exception compensationEx)
                    {
                        logger.LogCritical(compensationEx,
                            "Compensation failed for booking {BookingId}.", task.BookingId);
                    }
                }
            }
        }
    }
}