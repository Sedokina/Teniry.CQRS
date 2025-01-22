namespace Teniry.Cqrs.SampleApi.Application.CompleteTodo;

public class CompleteTodoCommand(Guid id) {
    public Guid Id { get; set; } = id;
}