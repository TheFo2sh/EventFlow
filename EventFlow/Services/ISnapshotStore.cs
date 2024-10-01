using EventFlow.Models;

namespace EventFlow.Services;

public interface ISnapshotStore<TState>
{
    Task<Snapshot<TState?>> GetSnapshotAsync(string primaryKey);
    Task SaveSnapshotAsync(string primaryKey, TState state, long version);
}