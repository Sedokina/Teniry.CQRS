using Teniry.Cqrs.Commands;

namespace Teniry.Cqrs.SampleApi.Application.Todos.CompleteTodo;

public class CompleteTodoHandler : ICommandHandler<CompleteTodoCommand> {
    private readonly TodoDb _db;

    public CompleteTodoHandler(TodoDb db) {
        _db = db;
    }

    /// <inheritdoc />
    public async Task HandleAsync(CompleteTodoCommand command, CancellationToken cancellation) {
        var todo = await _db.Todos.FindAsync([command.Id], cancellation);
        if (todo is null) {
            throw new InvalidOperationException("Todo not found");
        }

        todo.Completed = true;
        await _db.SaveChangesAsync(cancellation);
    }
}