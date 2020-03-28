using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.EventDetectors;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventExtensions
    {
        public static IServiceCollection AddEvents(this IServiceCollection services)
            => services.Scan(
                scan => scan
                    .FromAssemblyOf<UserRegisteredEventDetector>()
                    .AddClasses(classes => classes.AssignableTo(typeof(IEventDetector)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            );
    }
}