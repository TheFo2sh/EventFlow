using EventFlow.Models;
using EventFlow.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventFlow.ServiceCollection;

public class EventFlowBuilder
{
    private readonly IServiceCollection _services;

    public EventFlowBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Configures the EventFlow options.
    /// </summary>
    public EventFlowBuilder ConfigureOptions(Action<Configuration> configureOptions)
    {
        _services.Configure(configureOptions);
        return this;
    }

    /// <summary>
    /// Registers an ISnapshotStore for the specified state type.
    /// </summary>
    public EventFlowBuilder AddSnapshotStore<TState>(
        Func<IServiceProvider, ISnapshotStore<TState>> implementationFactory)
    {
        _services.AddSingleton<ISnapshotStore<TState>>(implementationFactory);
        return this;
    }

    /// <summary>
    /// Registers an IEventSource implementation.
    /// </summary>
    public EventFlowBuilder AddEventSource(Func<IServiceProvider, IEventSource> implementationFactory)
    {
        _services.AddSingleton<IEventSource>(implementationFactory);
        return this;
    }

    /// <summary>
    /// Registers a QueryModel and its dependencies.
    /// </summary>
    public EventFlowBuilder AddQueryModel<TState, TModel>()
        where TModel : QueryModel<TState>
    {
        // Register QueryModelServices<TState>
        _services.AddSingleton<QueryModelServices<TState>>(sp =>
        {
            var snapshotStore = sp.GetRequiredService<ISnapshotStore<TState>>();
            var eventSource = sp.GetRequiredService<IEventSource>();
            var options = sp.GetRequiredService<IOptions<Configuration>>();

            return new QueryModelServices<TState>(snapshotStore, eventSource, options);
        });

        // Register the QueryModel
        _services.AddSingleton<TModel>();

        return this;
    }
}