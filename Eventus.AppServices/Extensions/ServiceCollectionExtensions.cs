using Eventus.AppServices.Services.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Eventus.AppServices.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<IEventsService, EventsService>();
        return services;
    }
}