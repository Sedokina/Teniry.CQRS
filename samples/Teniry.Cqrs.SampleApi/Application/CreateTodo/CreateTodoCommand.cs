namespace Teniry.Cqrs.SampleApi.Application.CreateTodo;

public class CreateTodoCommand(string description) {
    public string Description { get; set; } = description;
}