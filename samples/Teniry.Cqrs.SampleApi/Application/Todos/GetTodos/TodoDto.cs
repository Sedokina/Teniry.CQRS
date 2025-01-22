namespace Teniry.Cqrs.SampleApi.Application.Todos.GetTodos;

public class TodoDto(string description, bool completed) {
    public string Description { get; set; } = description;
    public bool Completed { get; set; } = completed;
}