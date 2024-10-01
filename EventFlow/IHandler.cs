namespace EventFlow;

public interface IHandler<TState,TEvent>
{
    public string Correlate(TEvent evt);
    public Task<TState> HandleAsync(TState previousState,TEvent evt);
}