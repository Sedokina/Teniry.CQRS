using Teniry.Cqrs.Commands;
using Teniry.Cqrs.SampleApi.Domain;

namespace Teniry.Cqrs.SampleApi.Application.CreateTodo;

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