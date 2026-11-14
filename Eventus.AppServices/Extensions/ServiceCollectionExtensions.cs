using Eventus.AppServices.Queues.Bookings;
using Eventus.AppServices.Repositories.Bookings;
using Eventus.AppServices.Repositories.Events;
using Eventus.AppServices.Services.Bookings;
using Eventus.AppServices.Services.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Eventus.AppServices.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAppServices()
        {
            services.AddScoped<IEventsService, EventsService>();
            services.AddScoped<IBookingsService, BookingsService>();
            return services;
        }

        public IServiceCollection AddRepositories()
        {
            services.AddSingleton<IEventsRepository>(_ => new EventsInMemoryRepository([]));
            services.AddSingleton<IBookingsRepository>(_ => new BookingsInMemoryRepository([]));
            return services;
        }

        public IServiceCollection AddQueues()
        {
            services.AddSingleton<IBookingTaskQueue, BookingTaskInMemoryQueue>();
            return services;
        }
    }
}