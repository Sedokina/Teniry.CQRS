# Query

Query is a simple class, record or struct with no logic. It is used to pass data to the query handler.
It defines the data that can be used by query handler to select data from the database.

## Query handler

Query handler is a class that contains the logic to select data from the database. It should implement
`IQueryHandler<TQuery, TResult>` interface, where first parameter is the query which is handled by the handler and
second parameter is the result type.

`IQueryHandler<TQuery, TResult>` interface has only one method `HandleAsync` which is used to handle the query.
Generally query handler depends on the database context, or other services to select data. To inject them into the query
handler use constructor injection.

### Why does query handler have to implement `IQueryHandler<TQuery, TResult>` interface?

The `IQueryHandler<TQuery, TResult>` interface is used to find query handlers that has to be registered in the DI.
// TODO: add link to explanation of AddCQRS method

## Query dispatcher

Query dispatcher is a class that is used to 'connect' query to it's handler. It's internal implementation is responsible
for finding the handler that can handle the query and calling it's `HandleAsync` method.

To use query dispatcher you have to inject interface `IQueryDispatcher` into a class or method that heeds to call
the query handler.

Query dispatcher has only one method
`Task<TQueryResult> DispatchAsync<TQuery, TQueryResult>(TQuery query, CancellationToken cancellationToken)`.
Where `TQuery` is the query type and `TResult` is the result type that handler should return.

# Example
### Create query

For example, if we have todo list application, we can create a query to get all todos.

```csharp
public class GetTodosQuery { }
```

Any query can have properties to pass data to the query handler.
For example, our `GetTodosQuery` can contain a property to filter todos by description, can have pagination, sorting and
other properties.

For example:
We have a description property in the query which we will use to filter todos by description.

```csharp
public class GetTodosQuery {
    public string Description { get; set; }
}
```

> [!NOTE]  
> Query does not have to implement any interface or inherit any class to be able to call it's handler.

### Create query handler

If we have a query to get all todos, we have to create a query handler to select todos from the database.

For example, query handler could look like this:

```csharp
public class GetTodosHandler : IQueryHandler<GetTodosQuery, List<TodoDto>> {
    // We need a DbContext to select data from the database
    private readonly TodoDb _db;

    public GetTodosHandler(TodoDb db) {
        _db = db;
    }

    public async Task<PagedResult<TodoListItemDto>> HandleAsync(GetTodosQuery query, CancellationToken cancellationToken) {
        var result = await _db.Todos
            .Where(x => x.Description.Contains(query.Description))
            .Select(x => new TodoDto(x.TodoId, x.Description))
            .ToListAsync(cancellationToken);

        return result;
    }
}
```

### Dispatch query

When we use MinimalAPI we can create a method to handle the http request and dispatch the query. Otherwise, we need a
controller.

To dispatch the query we have to inject `IQueryDispatcher` into the method or controller.

For example, we can create a method like this:

```csharp
public static class Todos {
    public static async Task<IResult> GetTodosAsync(
        [AsParameters] GetTodosQuery query,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken
    ) {
        var result = await queryDispatcher
            .DispatchAsync<GetTodosQuery, List<TodoDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
```

Which then can be mapped to the endpoint in the `Program.cs` file.

```csharp
app.MapGet("todo", Todos.GetTodosAsync);
```

# Done
Now you have a query handler that can be used to select data from the database. Start the application and send a GET request
to 
```
/todo
```

to get the list of todos. You can also filter todos by description by sending a `description` query parameter like this 
```
/todo?description=milk
```