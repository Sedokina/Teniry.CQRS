# Teniry.CQRS

Allows you to develop your application faster by providing simple and lightweight CQRS implementation for .NET projects

Provides implementation for:

- Command and query segregation
- Automatic command and query handlers registration
- Command and query dispatching
- Event dispatching and handling
- Transactional command handlers
- Includes FluentValidation support for commands
- Types to simplify queries and commands implementation

Teniry.CQRS can be used with any database access library, such as Entity Framework, Dapper, etc. But it is recommended
to use it with Entity Framework Core, as it provides built-in transactional command handlers support.

Teniry.CQRS command and query dispatchers are fully implemented using Dependency Injection ✅,
and do not use any libraries like MediatR ❌.

# Installation

You can install the package via NuGet:

```
Install-Package Teniry.CQRS
```

# Get started

* Register CQRS services
* Implement command or query
* Dispatch it
* 🚀 Done! Use library to build any app you want

Add the following code to your `Program.cs` file, to register all necessary services for library to work

```csharp
// Add cqrs support
// Registers all necessary services including command and query dispatchers
// Automatically registers all command and query handlers in the assembly
builder.Services.AddCqrs();

// Add cqrs events support
builder.Services.AddApplicationEvents();
```

# Create command

Create a new class, record or struct that will be your command

```csharp
public class CreateTodoCommand(string description) {
    public string Description { get; set; } = description;
}
```

### Create command handler

Create a new class that will be your command handler

```csharp

public class CreateTodoHandler : ICommandHandler<CreateTodoCommand> {

    /// <inheritdoc />
    public async Task HandleAsync(CreateTodoCommand command, CancellationToken cancellation) {
        // provide real handler implementation here
        // save the todo to the database
    }
}
```

### Dispatch command

Create method to handle http request and dispatch the command

```csharp
public static class Endpoints {
    public static async Task<IResult> CreateTodoAsync(
        CreateTodoCommand command,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken
    ) {
        // Command can be created manually or fetched from reques body as in the example
        await commandDispatcher.DispatchAsync(command, cancellationToken);
        return TypedResults.Created();
    }
}
```

Map the endpoint to a route in the `Program.cs` file

```csharp
app.MapPost("todo/create", Endpoints.CreateTodoAsync);
```

### Done

Command handler is implemented and ready to use.

Start the application and send a POST request to `/todo/create` with a JSON body like this:

```json
{
  "description": "Buy milk"
}
```

# Create query

Create a new class, record or struct that will be your query

```csharp
public class GetTodosQuery { }
```

### Create query handler

Create a new class that will be your query handler

```csharp
public class GetTodosHandler : IQueryHandler<GetTodosQuery, List<TodoDto>> {
    /// <inheritdoc />
    public Task<List<TodoDto>> HandleAsync(GetTodosQuery query, CancellationToken cancellation) {
        // Implement the query handler here, select data from db
        
        // Example data
        var result = new List<TodoDto>() { new TodoDto(1, "Buy milk") };
        return result;
    }
}
```

### Dispatch query

Create method to handle http request and dispatch the query

```csharp
    public static async Task<IResult> GetTodosAsync(
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken
    ) {
        var result = await queryDispatcher
            .DispatchAsync<GetTodosQuery, List<TodoDto>>(new GetTodosQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }
```

Map the endpoint to a route in the `Program.cs` file

```csharp
app.MapGet("todo", Endpoints.GetTodosAsync);
```

### Done

Query handler is implemented and ready to use.

Start the application and send a GET request to `/todo` to get the list of todos.

