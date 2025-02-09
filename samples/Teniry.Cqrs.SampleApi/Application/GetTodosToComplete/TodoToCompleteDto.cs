namespace Teniry.Cqrs.SampleApi.Application.GetTodosToComplete;

public class TodoToCompleteDto(Guid id, string description) {
    public Guid Id { get; set; } = id;
    public string Description { get; set; } = description;
}