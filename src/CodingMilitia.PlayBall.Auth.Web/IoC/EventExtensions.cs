using CodingMilitia.PlayBall.Auth.Events;
using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.EventMappers;
using CodingMilitia.PlayBall.Auth.Web.Infrastructure.Events;
using CodingMilitia.PlayBall.Shared.EventBus;
using CodingMilitia.PlayBall.Shared.EventBus.Kafka;
using CodingMilitia.PlayBall.Shared.EventBus.Kafka.Configuration;
using CodingMilitia.PlayBall.Shared.EventBus.Serialization;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventExtensions
    {
        public static IServiceCollection AddEvents(this IServiceCollection services, IConfiguration configuration)
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
            services.AddSingleton<OutboxPublisher>();
            services.AddSingleton<OutboxFallbackPublisher>();

            services.AddHostedService<OutboxPublisherBackgroundService>();
            services.AddHostedService<OutboxPublisherFallbackBackgroundService>();

            services.AddTopicDistributor<BaseAuthEvent>(new[] {typeof(BaseUserEvent)});
            
            services.AddKafkaTopicPublisher(
                "UserAccountEvents",
                configuration.GetSection(nameof(KafkaSettings)).Get<KafkaSettings>(),
                Serializers.Utf8,
                JsonEventSerializer<BaseUserEvent>.Instance,
                @event => @event.UserId);

            return services;
        }
    }
}