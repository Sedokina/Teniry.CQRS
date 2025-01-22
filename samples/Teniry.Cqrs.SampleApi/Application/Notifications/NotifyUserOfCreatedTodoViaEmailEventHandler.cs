using Teniry.Cqrs.ApplicationEvents;
using Teniry.Cqrs.SampleApi.Application.CreateTodo;

namespace Teniry.Cqrs.SampleApi.Application.Notifications;

public class NotifyUserOfCreatedTodoViaEmailEventHandler : IApplicationEventHandler<TodoCreatedEvent> {
    private readonly ILogger<NotifyUserOfCreatedTodoViaEmailEventHandler> _logger;

    public NotifyUserOfCreatedTodoViaEmailEventHandler(ILogger<NotifyUserOfCreatedTodoViaEmailEventHandler> logger) {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(TodoCreatedEvent applicationEvent, CancellationToken cancellation) {
        // Send actual email via smtp service here

        // Simulate sending an email
        await Task.Delay(1000, cancellation);

        _logger.LogInformation("Email sent to user for todo: {Description}", applicationEvent.Description);
    }
}