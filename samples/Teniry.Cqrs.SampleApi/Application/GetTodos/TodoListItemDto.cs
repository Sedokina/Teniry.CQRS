namespace Teniry.Cqrs.SampleApi.Application.GetTodos;

public class TodoListItemDto(string description, bool completed) {
    public string Description { get; set; } = description;
    public bool Completed { get; set; } = completed;
}