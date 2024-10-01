using System.Collections.Immutable;
using EventFlow.Services;

namespace EventFlow.Test.Environment;

public class EmployeeProjectAllocationModel(QueryModelServices<EmployeeProjectAllocationModel.State> services) :
    QueryModel<EmployeeProjectAllocationModel.State>(services),
    IHandler<EmployeeProjectAllocationModel.State, ProjectEvents.EmployeeAssigned>,
    IHandler<EmployeeProjectAllocationModel.State, ProjectEvents.EmployeeUnassigned>
{
    public record State(ImmutableList<string> ProjectIds);

    protected override State GetInitialState() => new State(ImmutableList<string>.Empty);
    public string Correlate(ProjectEvents.EmployeeAssigned evt) => evt.EmployeeId;
    public string Correlate(ProjectEvents.EmployeeUnassigned evt) => evt.EmployeeId;


    public async Task<State> HandleAsync(State previousState, ProjectEvents.EmployeeAssigned evt)
    {
        await Task.Delay(1);
        return previousState with { ProjectIds = previousState.ProjectIds.Add(evt.ProjectId) };
    }


    public async Task<State> HandleAsync(State previousState, ProjectEvents.EmployeeUnassigned evt)
    {
        await Task.Delay(1);
        return previousState with { ProjectIds = previousState.ProjectIds.Remove(evt.ProjectId) };
    }
}