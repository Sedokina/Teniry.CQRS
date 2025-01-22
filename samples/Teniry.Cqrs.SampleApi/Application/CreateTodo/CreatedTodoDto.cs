namespace Teniry.Cqrs.SampleApi.Application.CreateTodo;

public class CreatedTodoDto(Guid id) {
    public Guid Id { get; set; } = id;
}