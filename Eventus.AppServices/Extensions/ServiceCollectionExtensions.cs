using Eventus.AppServices.Repositories.Events;
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
            return services;
        }

        public IServiceCollection AddRepositories()
        {
            services.AddSingleton<IEventsRepository>(_ => new EventsInMemoryRepository([]));
            return services;
        }
    }
}