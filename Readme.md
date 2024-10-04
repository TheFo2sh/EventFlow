
EventFlow Library
=================

EventFlow is a .NET library designed to simplify the implementation of event sourcing and CQRS (Command Query Responsibility Segregation) patterns. It provides a clean and efficient way to handle events, snapshots, and state reconstruction in applications that require high performance and scalability.

* * *

Table of Contents
-----------------

* [Features](#features)
* [Getting Started](#getting-started)
    * [Installation](#installation)
    * [Prerequisites](#prerequisites)
* [Usage](#usage)
    * [Creating a Query Model](#creating-a-query-model)
    * [Configuring Services](#configuring-services)
    * [Using the Query Model](#using-the-query-model)
* [Configuration](#configuration)
* [License](#license)

* * *

Features
--------

* **Event Sourcing Support**: Efficient handling and processing of events to reconstruct state.
* **Snapshot Mechanism**: Supports snapshots to improve performance by reducing the number of events to process.
* **Flexible Configuration**: Customize snapshot intervals and buffers.
* **Dependency Injection Friendly**: Easily integrate with `IServiceCollection` for registration and configuration.

* * *

Getting Started
---------------

### Installation

EventFlow is available as a NuGet package. You can install it using the Package Manager Console:

bash

Copy code

`Install-Package NEventFlow`

Or via the .NET CLI:

bash


`dotnet add package NEventFlow`

### Prerequisites

* **.NET 5.0** or later.
* Familiarity with event sourcing and CQRS patterns.
* An event storage solution for storing events and snapshots.

* * *

Usage
-----


### Creating a Query Model

Create a query model by extending the `QueryModel<TState>` class and implementing the `IHandler<TState, TEvent>` interface for each event type.



```csharp
    public class EmployeeProjectAllocationModel :
        QueryModel<EmployeeProjectAllocationModel.State>,
        IHandler<EmployeeProjectAllocationModel.State, ProjectEvents.EmployeeAssigned>,
        IHandler<EmployeeProjectAllocationModel.State, ProjectEvents.EmployeeUnassigned>
    {
        public record State(ImmutableList<string> ProjectIds);

        protected override State GetInitialState() => new State(ImmutableList<string>.Empty);

        public EmployeeProjectAllocationModel(QueryModelServices<State> services)
          : base(services)
        {}

        public string Correlate(ProjectEvents.EmployeeAssigned evt) => evt.EmployeeId;

        public string Correlate(ProjectEvents.EmployeeUnassigned evt) => evt.EmployeeId;

        public async Task<State> HandleAsync(State previousState, ProjectEvents.EmployeeAssigned evt)
        {
            await Task.CompletedTask;
            return previousState with { ProjectIds = previousState.ProjectIds.Add(evt.ProjectId) };
        }

        public async Task<State> HandleAsync(State previousState, ProjectEvents.EmployeeUnassigned evt)
        {
            await Task.CompletedTask;
            return previousState with { ProjectIds = previousState.ProjectIds.Remove(evt.ProjectId) };
        }
    }
```


### Configuring Services

Use the provided `EventFlowBuilder` to register services and configurations in your `Startup.cs` or `Program.cs`.


```csharp
using EventFlow;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add EventFlow and configure services
        services.AddEventFlow(builder =>
          {
            // Configure EventFlow options
            builder.ConfigureOptions(options => {
              options.SnapshotBuffer = 200;
              options.SnapshotInterval = TimeSpan.FromDays(7);
          })

        // Register ISnapshotStore for EmployeeProjectAllocationModel.State
         .AddSnapshotStore<EmployeeProjectAllocationModel.State>(sp =>  {
            // Implement and return your ISnapshotStore<TState>
              return new InMemorySnapshotStore<EmployeeProjectAllocationModel.State>();
          })
      
      // Register IEventSource
        .AddEventSource(sp => {
          // Implement and return your IEventSource
            return new InMemoryEventSource();
          })

    // Register the EmployeeProjectAllocationModel
      .AddQueryModel<EmployeeProjectAllocationModel.State, EmployeeProjectAllocationModel>();
    });
  }
}
```

**Note**: You need to implement `ISnapshotStore<TState>` and `IEventSource` according to your chosen storage mechanism. For example, you might create an in-memory implementation or use a database.

### Using the Query Model

Inject the `EmployeeProjectAllocationModel` into your services or controllers and use it to get the state.


```csharp
public class EmployeeService
{
    private readonly EmployeeProjectAllocationModel _model;

    public EmployeeService(EmployeeProjectAllocationModel model)
    {
        _model = model;
    }

    public async Task<EmployeeProjectAllocationModel.State> GetEmployeeProjectsAsync(string employeeId)
    {
        return await _model.GetStateAsync(employeeId);
    }
}
```
* * *

Configuration
-------------

EventFlow allows you to configure snapshot behavior via the `Configuration` class.


`public class Configuration
{
public int SnapshotBuffer { get; set; } = 100;
public TimeSpan SnapshotInterval { get; set; } = TimeSpan.FromDays(30);
}`

* **SnapshotBuffer**: The number of events to process before considering saving a new snapshot.
* **SnapshotInterval**: The time interval after which a new snapshot should be considered.

These settings can be configured when setting up the services:

```csharp
builder.ConfigureOptions(options =>
{
options.SnapshotBuffer = 200;
options.SnapshotInterval = TimeSpan.FromDays(7);
});
```
* * *

License
-------

EventFlow is licensed under the MIT License. See the LICENSE file for details.

* * *

Feel free to explore the library, and don't hesitate to reach out if you have any questions or need assistance!