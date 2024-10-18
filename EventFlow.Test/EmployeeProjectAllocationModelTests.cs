using System.Collections.Immutable;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Options;
using EventFlow.Models;
using EventFlow.Services;
using EventFlow.Test.Environment;

namespace EventFlow.Tests;

public class EmployeeProjectAllocationModelTests
{
    [Fact]
    public async Task Should_Add_ProjectId_When_EmployeeAssigned_Event()
    {
        // Arrange
        var snapshotStore = Substitute.For<ISnapshotStore<EmployeeProjectAllocationModel.State>>();
        snapshotStore.GetSnapshotAsync("employee123")
            .Returns(Task.FromResult(new Snapshot<EmployeeProjectAllocationModel.State>()
                { State = null, Version = 0 }));

        var events = new List<object>
        {
            new ProjectEvents.EmployeeAssigned("projectA", "employee123")
        };

        var eventSource = Substitute.For<IEventSource>();
        eventSource.GetEventsAsync("employee123", Arg.Any<IReadOnlyCollection<Type>>(), 0)
            .Returns(GetAsyncEnumerable(events));

        var configurationOptions = Options.Create(new Configuration());

        var services = new QueryModelServices<EmployeeProjectAllocationModel.State>(
            snapshotStore,
            eventSource,
            configurationOptions);

        var model = new EmployeeProjectAllocationModel(services);

        // Act
        var state = await model.GetStateAsync("employee123");

        // Assert
        state.ProjectIds.Should().Contain("projectA");
    }

    [Fact]
    public async Task Should_Remove_ProjectId_When_EmployeeUnassigned_Event()
    {
        // Arrange
        var initialState =
            new EmployeeProjectAllocationModel.State(ImmutableList<string>.Empty.Add("projectA").Add("projectB"));

        var snapshotStore = Substitute.For<ISnapshotStore<EmployeeProjectAllocationModel.State>>();
        snapshotStore.GetSnapshotAsync("employee123")
            .Returns(Task.FromResult(new Snapshot<EmployeeProjectAllocationModel.State>()
                { State = initialState, Version = 0 }));

        var events = new List<object>
        {
            new ProjectEvents.EmployeeUnassigned("projectA", "employee123")
        };

        var eventSource = Substitute.For<IEventSource>();
        eventSource.GetEventsAsync("employee123", Arg.Any<IReadOnlyCollection<Type>>(), 0)
            .Returns(GetAsyncEnumerable(events));

        var configurationOptions = Options.Create(new Configuration());

        var services = new QueryModelServices<EmployeeProjectAllocationModel.State>(
            snapshotStore,
            eventSource,
            configurationOptions);

        var model = new EmployeeProjectAllocationModel(services);

        // Act
        var state = await model.GetStateAsync("employee123");

        // Assert
        state.ProjectIds.Should().NotContain("projectA");
        state.ProjectIds.Should().Contain("projectB");
    }

    [Fact]
    public async Task Should_Handle_Multiple_Events_Correctly()
    {
        // Arrange
        var snapshotStore = Substitute.For<ISnapshotStore<EmployeeProjectAllocationModel.State>>();
        snapshotStore.GetSnapshotAsync("employee123")
            .Returns(Task.FromResult(new Snapshot<EmployeeProjectAllocationModel.State>()
                { State = null, Version = 0 }));

        var events = new List<object>
        {
            new ProjectEvents.EmployeeAssigned("projectA", "employee123"),
            new ProjectEvents.EmployeeAssigned("projectB", "employee123"),
            new ProjectEvents.EmployeeUnassigned("projectA", "employee123"),
            new ProjectEvents.EmployeeAssigned("projectC", "employee123")
        };

        var eventSource = Substitute.For<IEventSource>();
        eventSource.GetEventsAsync("employee123", Arg.Any<IReadOnlyCollection<Type>>(), 0)
            .Returns(GetAsyncEnumerable(events));

        var configurationOptions = Options.Create(new Configuration());

        var services = new QueryModelServices<EmployeeProjectAllocationModel.State>(
            snapshotStore,
            eventSource,
            configurationOptions);

        var model = new EmployeeProjectAllocationModel(services);

        // Act
        var state = await model.GetStateAsync("employee123");

        // Assert
        state.ProjectIds.Should().Contain("projectB");
        state.ProjectIds.Should().Contain("projectC");
        state.ProjectIds.Should().NotContain("projectA");
    }

    [Fact]
    public async Task Should_Use_Snapshot_As_Initial_State()
    {
        // Arrange
        var initialState = new EmployeeProjectAllocationModel.State(ImmutableList<string>.Empty.Add("projectExisting"));

        var snapshotStore = Substitute.For<ISnapshotStore<EmployeeProjectAllocationModel.State>>();
        snapshotStore.GetSnapshotAsync("employee123")
            .Returns(Task.FromResult(new Snapshot<EmployeeProjectAllocationModel.State>()
                { State = initialState, Version = 5 }));

        var events = new List<object>
        {
            new ProjectEvents.EmployeeAssigned("projectNew", "employee123")
        };

        var eventSource = Substitute.For<IEventSource>();
        eventSource.GetEventsAsync("employee123", Arg.Any<IReadOnlyCollection<Type>>(), 5)
            .Returns(GetAsyncEnumerable(events));

        var configurationOptions = Options.Create(new Configuration());

        var services = new QueryModelServices<EmployeeProjectAllocationModel.State>(
            snapshotStore,
            eventSource,
            configurationOptions);

        var model = new EmployeeProjectAllocationModel(services);

        // Act
        var state = await model.GetStateAsync("employee123");

        // Assert
        state.ProjectIds.Should().Contain("projectExisting");
        state.ProjectIds.Should().Contain("projectNew");
    }

    // Helper method to simulate asynchronous event streaming
    private static async IAsyncEnumerable<object> GetAsyncEnumerable(IEnumerable<object> items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.Delay(1); // Simulate asynchronous operation
        }
    }
}

