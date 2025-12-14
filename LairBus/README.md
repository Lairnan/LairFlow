# LairBus

[![Russian](https://img.shields.io/badge/Русский-RU-blue)](./README.ru.md)
[![NuGet version](https://img.shields.io/nuget/v/LairBus.svg?label=NuGet)](https://www.nuget.org/packages/LairBus)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/Lairnan/LairFlow)](LICENSE)

---

LairBus is a simple system for interacting via commands and notifications using a shared communication bus.

At the moment, it can be considered stable, but further improvements and new features will be added in the future as needed.

### Core concepts used in the project:

- `IBus`, `Bus` (main communication bus);
- `INotification`, `INotificationHandler` (interfaces for passing data via notifications);
- `IRequest`, `IRequestHandler` (interfaces for sending and handling commands with optional result data).

### Installation

```
Target framework: .NET 8 (works on higher versions as well)

Install via NuGet:
- dotnet add package LairBus

Install via Package Manager:
- Install-Package LairBus
```

---

## Usage examples

### Example 1

#### Registering LairBus in ServiceCollection

```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddBus(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
```

#### Implementing a simple command

```csharp
public class TestRequest : IRequest
{
    public int Id { get; set; }
}

public class TestRequestHandler : IRequestHandler<TestRequest>
{
    public async Task HandleRequest(TestRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Executing test request: {request.Id}");
    }
}
```

#### Executing a command and getting the result

```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddBus(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
var serviceProvider = serviceCollection.BuildServiceProvider();

var bus = serviceProvider.GetRequiredService<IBus>();
await bus.SendRequest(new TestRequest
{
    Id = 5
});

// Execution result:
// Executing test request: 5
```

### Example 2

#### Registering LairBus in ServiceCollection

```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddBus(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
```

#### Implementing a command with response object TestModel

```csharp
public class TestModel
{
    public long Id { get; set; }
    public string Text { get; set; }
}

public class TestResponseRequest : IRequest<TestModel>
{
    public long Id { get; set; }
    public string Input { get; set; }
}

public class TestResponseRequestHandler : IRequestHandler<TestResponseRequest, TestModel>
{
    public Task<TestModel> HandleRequest(TestResponseRequest request, CancellationToken cancellationToken)
    {
        var testModel = new TestModel
        {
            Id = request.Id,
            Text = request.Input
        };
        return Task.FromResult(testModel);
    }
}
```

#### Executing a command and getting the result

```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddBus(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
var serviceProvider = serviceCollection.BuildServiceProvider();

var model = await bus.SendRequest<TestModel>(new TestResponseRequest
{
    Id = 5,
    Input = "TEST INPUT"
});
Console.WriteLine($"Id: {model.Id}, text: {model.Text}");

// Execution result:
// Id: 5, text: TEST INPUT
```

---

## Compatibility

- .NET 8+

## License

The project is licensed under the Apache License.
See the [LICENSE](./LICENSE) file for details.