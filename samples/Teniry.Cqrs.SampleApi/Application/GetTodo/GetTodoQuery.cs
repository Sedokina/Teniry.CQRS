namespace Teniry.Cqrs.SampleApi.Application.GetTodo;

public class GetTodoQuery(Guid id) {
    public Guid Id { get; set; } = id;
}