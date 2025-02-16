# Command

Command is a simple class, record or struct with no logic. It represents the action to be taken. It is used to pass data
to the command handler.

# Command handler without return value

Command handler is a class that contains the logic to execute the command. It should implement
`ICommandHandler<TCommand>` interface, where parameter is the command which is handled by the handler.

`ICommandHandler<TCommand>` interface has only one method `HandleAsync` which is used to handle the command.
Generally command handler depends on the database context, or other services to add or update data. To inject them into
the command handler use constructor injection.

# Command handler with return value

Generally, command handler does not return any value. But sometimes you need to return some value from the command
handler, for example Id of the entity that was created on the database side.

To return value from the command handler, you can use `ICommandHandler<TCommand, TResult>` interface, where first
parameter
is the command which is handled by the handler and second parameter is the result type.

The `HandleAsync` method of the `ICommandHandler<TCommand, TResult>` interface should return the result of the command
execution.

### Why does command handler have to implement `ICommandHandler<TCommand>` or

`ICommandHandler<TCommand, TResult>` interface?

The `ICommandHandler` interface is used to find command handlers that has to be registered in the DI.
// TODO: add link to explanation of AddCQRS method

# Command Dispatcher

Command dispatcher is a class that is used to 'connect' command to it's handler. It's internal implementation is
responsible
for finding the handler that can handle the command and calling it's `HandleAsync` method.

To use command dispatcher you have to inject interface `ICommandDispatcher` into a class or method that heeds to call
the command handler.

Depending on the return type of the command handler, command dispatcher has two methods:

1. `Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken)` - for command handlers without
   return value.
2.

`Task<TCommandResult> DispatchAsync<TCommand, TCommandResult>(TCommand command, CancellationToken cancellationToken)` -
for command handlers with return value.

# Command validation

Every user input should be validated. Command validation is a way to validate the command before it is passed to the
command handler.

To validate the command you can create a class that implements abstract class `AbstractValidator<TCommand>` this class
provided by the FluentValidation library and integrates with the Teniry.CQRS library.

Command dispatcher is automatically calls the validator before calling the command handler. You don't need to register
the validator in the DI container, because it is done automatically.

If the command is not valid, the command dispatcher throws `ValidationException` with the list of errors.

More on FluentValidation library can be found [here](https://docs.fluentvalidation.net/en/latest/).

# Transactional command handler

Command handlers can require a transaction to be able to save multiple records to the database.
To make the command handler transactional, you can apply the `ITransactionalHandler` interface to the command handler.

`ITransactionalHandler` interface is a marker interface that tells the command dispatcher that the command handler
requires a transaction to be able to save data to the database.

Command dispatcher automatically starts a transaction before calling the command handler and commits it after the
handler has finished.

That way, you don't need to call `BeginTransactionAsync` or `CommitTransactionAsync` methods in the command handler.

> [!WARNING]
> Currently only RepeatableRead isolation level is supported

# Example

### Create command

For example if we have todo list application, we can create a command to create a new todo.

```csharp
public class CreateTodoCommand(string description) {
    public string Description { get; set; } = description;
}
```

Any command can have properties to pass data to the command handler.

For example, our `CreateTodoCommand` can contain a property to set the description of the todo.


> [!NOTE]  
> Command does not have to implement any interface or inherit any class to be able to call it's handler.

### Create command handler

If we have a command to create a new todo, we have to create a command handler to save the todo to the database.

For example, command handler could look like this:

```csharp
public class CreateTodoHandler : ICommandHandler<CreateTodoCommand, CreatedTodoDto> {
    private readonly TodoDb _db;

    public CreateTodoHandler(TodoDb db) {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<CreatedTodoDto> HandleAsync(CreateTodoCommand command, CancellationToken cancellation) {
        var todo = new Todo(command.Description, false);
        await _db.Todos.AddAsync(todo, cancellation);
        await _db.SaveChangesAsync(cancellation);
        return new(todo.Id);
    }
}
```

```csharp
public class CreatedTodoDto(Guid id) {
    public Guid Id { get; set; } = id;
}
```

It creates a new todo with the description provided in the command, saves it to the database and returns
the Id of the created todo.

Same handler but without return value could look like this:

```csharp
public class CreateTodoHandler : ICommandHandler<CreateTodoCommand> {
    private readonly TodoDb _db;

    public CreateTodoHandler(TodoDb db) {
        _db = db;
    }

    /// <inheritdoc />
    public async Task HandleAsync(CreateTodoCommand command, CancellationToken cancellation) {
        var todo = new Todo(command.Description, false);
        await _db.Todos.AddAsync(todo, cancellation);
        await _db.SaveChangesAsync(cancellation);
        return new(todo.Id);
    }
}
```

The main difference is that the method `HandleAsync` does not return any value, and `ICommandHandler` interface has only
one generic parameter.

### Create transactional handler

If you need to make CreateTodoHandler transactional, you can apply the `ITransactionalHandler` interface to the command.
It can be modified like this:

```csharp
public class CreateTodoHandler : ICommandHandler<CreateTodoCommand, CreatedTodoDto>, ITransactionalHandler {
    private readonly TodoDb _db;

    public CreateTodoHandler(TodoDb db) {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<CreatedTodoDto> HandleAsync(CreateTodoCommand command, CancellationToken cancellation) {
        var todo = new Todo(command.Description, false);
        await _db.Todos.AddAsync(todo, cancellation);
        await _db.SaveChangesAsync(cancellation);
        
        // Then save some other data to the database, for example logs
        var log = new Log("Todo created with id: " + todo.Id);
        await _db.Logs.AddAsync(log, cancellation);
        await _db.SaveChangesAsync(cancellation);
        
        return new(todo.Id);
    }
}
```

### Create validator

To validate the command you have to create a validator class that implements abstract class
`AbstractValidator<TCommand>`
For example, the validator for the `CreateTodoCommand` could look like this:

```csharp
public class CreateTodoCommandValidator : AbstractValidator<CreateTodoCommand> {
    public CreateTodoCommandValidator() {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(100);
    }
}
```

### Dispatch command

When we use MinimalAPI we can create a method to handle the http request and dispatch the command. Otherwise, we need a
controller.

To dispatch the query we have to inject `ICommandDispatcher` into the method or controller.

For example, we can create a method to call handler with return value:

```csharp
public static class Todos {
    public static async Task<IResult> CreateTodoAsync(
        CreateTodoCommand command,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken
    ) {
        var result = await commandDispatcher
            .DispatchAsync<CreateTodoCommand, CreatedTodoDto>(command, cancellationToken);

        return TypedResults.Created($"todo/{result.Id}");
    }
}
```

Or if our handler does not return any value:

```csharp
public static class Endpoints {
    public static async Task<IResult> CreateTodoAsync(
        CreateTodoCommand command,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken
    ) {
        await commandDispatcher.DispatchAsync(command, cancellationToken);
        return TypedResults.Created();
    }
}
```

> [!NOTE]
> Command is just a simple model with no logic. That is why it can be created manually with `new` keyword or fetched
> from the request body.

Map the endpoint to a route in the `Program.cs` file and you are ready to go.

```csharp
app.MapPost("todo/create", Endpoints.CreateTodoAsync);
```

### Done

Now you have a command handler that can be used to create new todos and save it to the database. Start the application
and send a POST request to

```
/todo/create
```

with a JSON body like this:

```json
{
  "description": "Buy milk"
}
```