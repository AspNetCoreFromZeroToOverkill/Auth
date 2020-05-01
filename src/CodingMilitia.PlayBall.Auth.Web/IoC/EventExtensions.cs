using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.EventMappers;
using CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events;

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

            services.AddHostedService<OutboxPublisherBackgroundService>();
            services.AddHostedService<OutboxPublisherFallbackBackgroundService>();

            return services;
        }
    }
}