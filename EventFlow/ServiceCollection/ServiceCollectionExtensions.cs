using Microsoft.Extensions.DependencyInjection;

namespace EventFlow.ServiceCollection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventFlow(this IServiceCollection services, Action<EventFlowBuilder> configure)
    {
        var builder = new EventFlowBuilder(services);
        configure(builder);
        return services;
    }
}