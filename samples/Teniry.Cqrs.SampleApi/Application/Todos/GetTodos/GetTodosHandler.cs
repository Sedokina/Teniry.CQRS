using Microsoft.EntityFrameworkCore;
using Teniry.Cqrs.Extended.Queryables.Filter;
using Teniry.Cqrs.Queries;

namespace Teniry.Cqrs.SampleApi.Application.Todos.GetTodos;

public class GetTodosHandler : IQueryHandler<GetTodosQuery, List<TodoDto>> {
    private readonly TodoDb _db;

    public GetTodosHandler(TodoDb db) {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<List<TodoDto>> HandleAsync(GetTodosQuery query, CancellationToken cancellation) {
        // Filter is a feature of Teniry.Cqrs.Extended package
        var filter = new TodosFilter {
            Description = query.Description
        };

        var result = await _db.Todos
            .Filter(filter)
            .Select(x => new TodoDto(x.Description, x.Completed))
            .ToListAsync(cancellation);

        return result;
    }
}