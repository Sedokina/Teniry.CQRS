using Microsoft.EntityFrameworkCore;
using Teniry.Cqrs.Queries;
using Teniry.Cqrs.SampleApi.Application.GetTodos;

namespace Teniry.Cqrs.SampleApi.Application.GetTodosToComplete;

/// <summary>
///     This is an example of simple list query handler
///     for complex query handler with filters and paging see <see cref="GetTodosHandler"/>
/// </summary>
public class GetTodosToCompleteHandler : IQueryHandler<GetTodosToCompleteQuery, List<TodoToCompleteDto>> {
    private readonly TodoDb _todoDb;

    public GetTodosToCompleteHandler(TodoDb todoDb) {
        _todoDb = todoDb;
    }

    /// <inheritdoc />
    public Task<List<TodoToCompleteDto>> HandleAsync(GetTodosToCompleteQuery query, CancellationToken cancellation) {
        var result = _todoDb.Todos
            .Where(x => !x.Completed)
            .Select(x => new TodoToCompleteDto(x.Id, x.Description))
            .ToListAsync(cancellation);

        return result;
    }
}