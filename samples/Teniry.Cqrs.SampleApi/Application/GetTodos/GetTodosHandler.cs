using Teniry.Cqrs.Extended.Queryables.Filter;
using Teniry.Cqrs.Extended.Queryables.Page;
using Teniry.Cqrs.Queries;

namespace Teniry.Cqrs.SampleApi.Application.GetTodos;

/// <remark>
///     PagedResult is a class of Teniry.Cqrs.Extended package
/// </remark>
public class GetTodosHandler : IQueryHandler<GetTodosQuery, PagedResult<TodoListItemDto>> {
    private readonly TodoDb _db;

    public GetTodosHandler(TodoDb db) {
        _db = db;
    }

    /// <inheritdoc />
    /// <remark>
    ///     .Filter(...) is a Linq extension of Teniry.Cqrs.Extended package
    ///     .ToPagedListAsync(...) is a Linq extension of Teniry.Cqrs.Extended package
    /// </remark>
    public async Task<PagedResult<TodoListItemDto>> HandleAsync(GetTodosQuery query, CancellationToken cancellation) {
        var filter = new TodosFilter {
            Description = query.Description,
            Sort = query.Sort
        };

        var result = await _db.Todos
            .Filter(filter)
            .Select(x => new TodoListItemDto(x.Description, x.Completed))
            .ToPagedListAsync(query, cancellation);

        return new(result.ToList(), result.GetPage());
    }
}