using Teniry.Cqrs.Queries;

namespace Teniry.Cqrs.SampleApi.Application.GetTodo;

public class GetTodoHandler : IQueryHandler<GetTodoQuery, TodoDto> {
    private readonly TodoDb _db;

    public GetTodoHandler(TodoDb db) {
        _db = db;
    }

    public async Task<TodoDto> HandleAsync(GetTodoQuery query, CancellationToken cancellationToken) {
        var todo = await _db.Todos.FindAsync([query.Id], cancellationToken);

        if (todo is null) {
            throw new InvalidOperationException("Todo not found");
        }

        return new(todo.Id, todo.Description, todo.Completed);
    }
}