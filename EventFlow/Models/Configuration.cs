namespace EventFlow.Models;

public class Configuration
{
    public int SnapshotBuffer { get; set; }=100;
    public TimeSpan SnapshotInterval { get; set; } = TimeSpan.FromDays(30);
}