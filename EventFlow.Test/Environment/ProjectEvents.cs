namespace EventFlow.Test.Environment;

public abstract record ProjectEvents(string ProjectId,DateTime TimeStamp)
{
    public record ProjectCreated(string ProjectId,string ProjectName, string ProjectDescription) : ProjectEvents(ProjectId,DateTime.Now);
    public record ProjectUpdated(string ProjectId,string ProjectName, string ProjectDescription) : ProjectEvents(ProjectId,DateTime.Now);
    public record ProjectDeleted(string ProjectId) : ProjectEvents(ProjectId,DateTime.Now);
    public record EmployeeAssigned(string ProjectId, string EmployeeId) : ProjectEvents(ProjectId,DateTime.Now);
    public record EmployeeUnassigned(string ProjectId, string EmployeeId) : ProjectEvents(ProjectId,DateTime.Now);
}