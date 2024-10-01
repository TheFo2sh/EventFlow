namespace EventFlow.Test.Environment;

public abstract record EmployeeEvents(string EmployeeId,DateTime TimeStamp)
{
    public record EmployeeCreated(string EmployeeId,string EmployeeName, string EmployeeDescription) : EmployeeEvents(EmployeeId,DateTime.Now);
    public record EmployeeUpdated(string EmployeeId,string EmployeeName, string EmployeeDescription) : EmployeeEvents(EmployeeId,DateTime.Now);
    public record EmployeeDeleted(string EmployeeId) : EmployeeEvents(EmployeeId,DateTime.Now);
}