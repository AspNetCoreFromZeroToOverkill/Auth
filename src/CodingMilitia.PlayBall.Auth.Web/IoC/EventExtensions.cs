using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.EventMappers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventExtensions
    {
        public static IServiceCollection AddEvents(this IServiceCollection services)
            => services.Scan(
                scan => scan
                    .FromAssemblyOf<UserRegisteredEventMapper>()
                    .AddClasses(classes => classes.AssignableTo(typeof(IEventMapper)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
            );
    }
}