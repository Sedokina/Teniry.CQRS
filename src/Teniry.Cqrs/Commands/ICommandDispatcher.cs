namespace Teniry.Cqrs.Commands;

public interface ICommandDispatcher {
    Task DispatchAsync<TCommand>(TCommand                                 command, CancellationToken cancellation);
    Task<TCommandResult> DispatchAsync<TCommand, TCommandResult>(TCommand command, CancellationToken cancellation);
}