namespace EventFlow.Models;

public class Snapshot<TState>
{
    public TState State { get; set; }
    public long Version { get; set; }
    
}