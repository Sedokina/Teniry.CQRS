# Extended queries

Teniry.CQRS.Extended package provides a set of types to simplify queries implementation.

### Pagination

Teniry.CQRS.Extended includes `X.PagedList` library to provide pagination support for queries. On top of that, it
provides a set of types to simplify pagination implementation, such as:

* `IPage` - interface that can be implemented by query to provide pagination support
* `ToPagedListAsync` - extension method to apply pagination on IQueryable, accepts any class that implements `IPage`
* `PagedResult<T>` - class that contains data itself and metadata about pagination
* `PageInfo` - class that contains metadata about pagination

> [!NOTE]
> To convert Result of `ToPagedListAsync` to `PageInfo` use `GetPage` extension method.

### Sorting

Teniry.CQRS.Extended provides a set of types to simplify sorting implementation:

* `IDefineSortable` - interface that can be implemented by query indicate that the query supports sorting, and provide
  list of property names that can be sorted

### Filtering

Teniry.CQRS.Extended provides `QueryableFilter<TEntity>` abstract class to simplify filtering. It provides basic
implementation for applying filtering and sorting. It has two abstract methods that should be implemented by the derived
class:

* `Dictionary<string, Expression<Func<TEntity, object>>> DefineSort();` - should return dictionary where
  key is the property name (same as in the IDefineSortable defined in the query) and value is the expression for the
  property of `TEntity` type
* `IQueryable<TEntity> Filter(IQueryable<TEntity> query);` - method that contains the filtering logic
* `IQueryable<TEntity> DefaultSort(IQueryable<TEntity> query)` - optional method that contains default sorting logic

The `QueryableFilter<TEntity>` is targeted on IQueryable queries. To apply filtering and sorting, to IQueryable use
`Filter(...)` extension method over `IQueryable`.

# Example

We would use same example as in the [Queries][/docs/queries] section.

### Create query

```csharp
public class GetTodosQuery : IPage, IDefineSortable {
    public string? Description { get; set; }

    /// <inheritdoc />
    public int Page { get; set; }

    /// <inheritdoc />
    public int PageSize { get; set; }

    /// <inheritdoc />
    public string[]? Sort { get; set; } // accepts sort keys that should be applied to query

    /// <inheritdoc />
    public string[] GetSortKeys() {
        return ["description", "completed"]; // defines what keys can be used for sorting
    }
}
```

### Create query handler

```csharp
public class GetTodosHandler : IQueryHandler<GetTodosQuery, PagedResult<TodoDto>> {
    private readonly TodoDb _db;

    public GetTodosHandler(TodoDb db) {
        _db = db;
    }

    public async Task<PagedResult<TodoDto>> HandleAsync(GetTodosQuery query, CancellationToken cancellationToken) {
        // create filter, code for TodosFilter class is below
        var filter = new TodosFilter {
            Description = query.Description,
            Sort = query.Sort
        };

        var result = await _db.Todos
            .Filter(filter) // apply filter
            .Select(x => new TodoDto(x.Description, x.Completed))
            .ToPagedListAsync(query, cancellationToken);

        return new(result.ToList(), result.GetPage());
    }
}
```

### Create filter

```csharp
public class TodosFilter : QueryableFilter<Todo> {
    public string? Description { get; set; }

    /// <inheritdoc />
    public override Dictionary<string, Expression<Func<Todo, object>>> DefineSort() {
        return new() {
            { "description", x => x.Description },
            { "completed", x => x.Completed }
        };
    }

    /// <inheritdoc />
    protected override IQueryable<Todo> DefaultSort(IQueryable<Todo> query) {
        return query.OrderBy(x => x.Description);
    }

    /// <inheritdoc />
    protected override IQueryable<Todo> Filter(IQueryable<Todo> query) {
        if (Description != null) {
            query = query.Where(x => EF.Functions.ILike(x.Description, $"%{Description}%"));
        }

        return query;
    }
}
```

### Dispatch query

Dispatch stays the same as in the [Queries][/docs/queries] section. Because only inner implementation for filtering and
sorting changed

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

```csharp
app.MapGet("todo", Todos.GetTodosAsync);
```

### Call the route

Route `/todo` can be called with query parameters `description`, `page`, `pageSize` and `sort`. For example:

```
GET /todo?description=milk&page=1&pageSize=10&sort=asc.completed
```

Sorting order can be changed by changing `asc` to `desc`.