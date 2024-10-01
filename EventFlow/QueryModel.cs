using System.Linq.Expressions;
using EventFlow.Models;
using EventFlow.Services;

namespace EventFlow;

public abstract class QueryModel<TState>
{
    private readonly QueryModelServices<TState> _services;
    private readonly Dictionary<Type, Func<object, string>> _correlationFuncs = new();
    private readonly Dictionary<Type, Func<TState, object, Task<TState>>> _handlerFuncs = new();

    protected QueryModel(QueryModelServices<TState> services)
    {
        _services = services;
        RegisterHandlers();
    }

    public async Task<TState> GetStateAsync(string primaryKey)
    {
        var snapshot = await _services.SnapshotStore.GetSnapshotAsync(primaryKey);
        var events = _services.EventSource.GetEventsAsync(primaryKey,_correlationFuncs.Keys, snapshot.Version);
        var state = snapshot.State ?? GetInitialState();
        long eventsCount = 0;
        await foreach (var evt in events)
        {
            var eventType = evt.GetType();

            if (!_correlationFuncs.TryGetValue(eventType, out var correlateFunc))
                continue; // No correlation function found; skip event

            var correlationId = correlateFunc(evt);
            if (correlationId != primaryKey)
                continue; // Event does not match primary key; skip

            if (!_handlerFuncs.TryGetValue(eventType, out var handlerFunc))
                continue; // No handler function found; skip event

            state = await handlerFunc(state, evt);
            eventsCount++;
        }

        await UpdateSnapshot(primaryKey, eventsCount, snapshot, state);

        return state;
    }

    private async Task UpdateSnapshot(string primaryKey, long eventsCount, Snapshot<TState?> snapshot, TState state)
    {
        var lastSnapshotTime = DateTimeOffset.FromUnixTimeMilliseconds(snapshot.Version);
        var timeSinceLastSnapshot = DateTimeOffset.UtcNow - lastSnapshotTime;

        var shouldSaveSnapshot =
            eventsCount >= _services.Options.Value.SnapshotBuffer ||
            timeSinceLastSnapshot >= _services.Options.Value.SnapshotInterval;

        if (shouldSaveSnapshot)
        {
            await _services.SnapshotStore.SaveSnapshotAsync(primaryKey, state, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }


    protected abstract TState GetInitialState();


    private void RegisterHandlers()
    {
        var interfaces = GetType().GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandler<,>));

        foreach (var iface in interfaces)
        {
            var stateType = iface.GetGenericArguments()[0];
            var eventType = iface.GetGenericArguments()[1];

            // Ensure state type matches
            if (stateType != typeof(TState))
                continue;

            // Get methods
            var correlateMethod = iface.GetMethod("Correlate");
            var handleMethod = iface.GetMethod("HandleAsync");

            if (correlateMethod == null || handleMethod == null)
                continue;

            // Create the correlateDelegate
            {
                var eventParam = Expression.Parameter(typeof(object), "evt");
                var instance = Expression.Constant(this);
                var methodCall = Expression.Call(
                    instance,
                    correlateMethod,
                    Expression.Convert(eventParam, eventType));

                var lambda = Expression.Lambda<Func<object, string>>(methodCall, eventParam);
                var correlateDelegate = lambda.Compile();

                _correlationFuncs[eventType] = correlateDelegate;
            }

            // Create the handlerDelegate
            {
                var stateParam = Expression.Parameter(typeof(TState), "state");
                var eventParam = Expression.Parameter(typeof(object), "evt");
                var instance = Expression.Constant(this);
                var methodCall = Expression.Call(
                    instance,
                    handleMethod,
                    stateParam,
                    Expression.Convert(eventParam, eventType));

                var lambda = Expression.Lambda<Func<TState, object, Task<TState>>>(methodCall, stateParam, eventParam);
                var handlerDelegate = lambda.Compile();

                _handlerFuncs[eventType] = handlerDelegate;
            }
        }
    }

}