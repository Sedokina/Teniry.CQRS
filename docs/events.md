# Events

Events are a way to notify other parts of the application that something has happened. They are a way to decouple
different parts of the application.

Event is a simple class or record with no logic. It is used to pass data to the event handler.
Any event has to implement `IApplicationEvent` interface.

# Event handler

Event handler is a class that contains the logic to handle the event. It should implement
`IApplicationEventHandler<TApplicationEvent>` interface, where parameter is the event which is handled by the handler.

`IApplicationEventHandler<TApplicationEvent>` interface has only one method `HandleAsync` which is used to handle the
event. The `HandleAsync` method can not have a return value.
Generally event handler depends on services like SMTP. To inject them into the command handler use constructor
injection.

### Why does event handler have to implement `IApplicationEventHandler<TApplicationEvent>` interface?

The `IApplicationEventHandler` interface is used to find event handlers that has to be registered in the DI,
see [Register event services](register-cqrs.md#register-event-services) for details.

# Trigger event

Every command handler can trigger events. For command handler to be able to trigger events it has to implement
`IApplicationEventTrigger` interface.

`IApplicationEventTrigger` interface has one field that has to be implemented in the handler

```
event ApplicationEventSubscriber ApplicationEvent;
```

`ApplicationEvent` is an event that can be invoked in the command handler with `Invoke` method.

> [!NOTE]
> Invoke method can be called safely as it does not call the event handler directly, but it adds the event to the queue
> of events that will be processed after the command handler is finished.

# Example

### Create event

For example if we have todo list application, and a command to create a new todo. We can require to notify our user when
the todo is created.

For that we can create an event `TodoCreatedEvent` that will be triggered when the todo is created.

```csharp
public class TodoCreatedEvent(string description) : IApplicationEvent {
    public string Description { get; set; } = description;
}
```

Any event can have properties to pass data to the event handler.
For example, our `TodoCreatedEvent` can contain a property to set the description of the todo.

### Create event handler

To implement user notification we have to create an event handler.

```csharp
public class NotifyUserOfCreatedTodoViaEmailEventHandler : IApplicationEventHandler<TodoCreatedEvent> {
    private readonly IMySmtpClient _smtpClient;

    public NotifyUserOfCreatedTodoViaEmailEventHandler(IMySmtpClient smtpClient) {
        _smtpClient = smtpClient;
    }

    /// <inheritdoc />
    public async Task HandleAsync(TodoCreatedEvent applicationEvent, CancellationToken cancellation) {
        await _smtpClient.SendEmailToUserAsync(applicationEvent.Description);
    }
}
```

It sends an email to the user with the description of the created todo.

### Trigger event

To trigger the event in the command handler we have to implement `IApplicationEventTrigger` interface.
The `CreateTodoHandler` can be modified to look like this:

```csharp
public class CreateTodoHandler : ICommandHandler<CreateTodoCommand, CreatedTodoDto>, IApplicationEventTrigger {
    private readonly TodoDb _db;

    /// <inheritdoc />
    public event ApplicationEventSubscriber ApplicationEvent; // This a part of IApplicationEventTrigger and required to trigger events

    public CreateTodoHandler(TodoDb db) {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<CreatedTodoDto> HandleAsync(CreateTodoCommand command, CancellationToken cancellation) {
        var todo = new Todo(command.Description, false);
        await _db.Todos.AddAsync(todo, cancellation);
        await _db.SaveChangesAsync(cancellation);

        // Trigger event when the todo is created
        await ApplicationEvent.Invoke(this, new TodoCreatedEvent(todo.Description), cancellation);

        return new(todo.Id);
    }
}
```