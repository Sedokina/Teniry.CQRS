namespace Teniry.Cqrs.SampleApi.Application.Todos.CreateTodo;

public class CreateTodoCommand(string description) {
    public string Description { get; set; } = description;
}