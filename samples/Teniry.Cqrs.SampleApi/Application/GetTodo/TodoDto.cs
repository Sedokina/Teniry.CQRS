namespace Teniry.Cqrs.SampleApi.Application.GetTodo;

public class TodoDto(Guid id, string description, bool completed) {
    public Guid Id { get; set; } = id;
    public string Description { get; set; } = description;
    public bool Completed { get; set; } = completed;
}