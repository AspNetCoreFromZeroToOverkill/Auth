using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events;
using CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Events.Mappers;
using CodingMilitia.PlayBall.Shared.EventBus;
using EventContracts = CodingMilitia.PlayBall.Auth.Events;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventExtensions
    {
        public static IServiceCollection AddEvents(this IServiceCollection services)
        {
            services.Scan(
                scan => scan
                    .FromAssemblyOf<UserRegisteredEventMapper>()
                    .AddClasses(classes => classes.AssignableTo(typeof(IEventMapper)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            );

            services.AddSingleton<OutboxListener>();
            services.AddSingleton<OnNewOutboxMessages>(s => s.GetRequiredService<OutboxListener>().OnNewMessages);
            services.AddScoped<OutboxPublisher>();
            services.AddScoped<OutboxFallbackPublisher>();
            
            // stub to simulate actual bus implementation
            services.AddSingleton<IEventPublisher<EventContracts.BaseAuthEvent>, TempEventBusPublisher>();

            services.AddHostedService<OutboxBackgroundService>();
            services.AddHostedService<OutboxFallbackBackgroundService>();

            return services;
        }
    }
}