using EventFlow.Models;
using Microsoft.Extensions.Options;

namespace EventFlow.Services;

public record QueryModelServices<TState>(ISnapshotStore<TState> SnapshotStore, IEventSource EventSource, IOptions<Configuration> Options);