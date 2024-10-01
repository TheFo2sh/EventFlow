namespace EventFlow.Services;

public interface IEventSource
{
    IAsyncEnumerable<object> GetEventsAsync(string primaryKey,IReadOnlyCollection<Type> events, long fromVersion);
}