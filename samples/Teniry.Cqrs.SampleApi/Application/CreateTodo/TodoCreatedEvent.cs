using Teniry.Cqrs.ApplicationEvents;

namespace Teniry.Cqrs.SampleApi.Application.CreateTodo;

public class TodoCreatedEvent(string description) : IApplicationEvent {
    public string Description { get; set; } = description;
}