namespace Teniry.Cqrs.SampleApi.Application.Todos.CompleteTodo;

public class CompleteTodoCommand(Guid id) {
    public Guid Id { get; set; } = id;
}