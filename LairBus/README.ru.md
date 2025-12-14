# LairBus

[![English](https://img.shields.io/badge/English-US-blue)](./README.md)
[![NuGet version](https://img.shields.io/nuget/v/LairBus.svg?label=NuGet)](https://www.nuget.org/packages/LairBus)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/Lairnan/LairFlow)](LICENSE)

---

LairBus - это простая система взаимодействия командами и уведомлениями через общую шину связи.

На текущий момент можно считать стабильной версией, но в будущем будут доработки и добавление нового функционала по мере необходимости.

### Какие объекты использует проект:

- IBus, Bus (Главная шина взаимодействия);
- INotification, INotificationHandler (Интерфейс для передачи данных через уведомления);
- IRequest, IRequestHandler (Интерфейс отправки и выполнения команд с возвратом необходимых данных).

### Установка для использования

```
Целевая библиотека .NET 8, но работает и выше

Установить через NuGet:
- dotnet add package LairBus

Установить через менеджер пакетов:
- Install-Package LairBus
```

---

## Примеры использования

### Пример 1

#### Регистрация LairBus в ServiceCollection

```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddBus(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
```

#### Реализация простой команды

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

#### Выполнение команды и получение результата

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

// Результат выполнения
// Executing test request: 5
```

---

### Пример 2

#### Регистрация LairBus в ServiceCollection

```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddBus(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
```

#### Реализация команды с возвращаемой объектом TestModel

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

#### Выполнение команды и получение результата

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

// Результат выполнения
// Id: 5, text: TEST INPUT
```

## Совместимость

- .NET 8+

## Лицензия

Проект лицензирован под лицензией Apache.
Подробности в файле [LICENSE](./LICENSE)