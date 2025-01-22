namespace Teniry.Cqrs.SampleApi.Application.Todos.CreateTodo;

public class CreatedTodoDto(Guid id) {
    public Guid Id { get; set; } = id;
}