namespace Teniry.Cqrs.Commands;

public interface ICommandHandler<in TCommand, TCommandResult> {
    Task<TCommandResult> HandleAsync(TCommand command, CancellationToken cancellation);
}

public interface ICommandHandler<in TCommand> {
    Task HandleAsync(TCommand command, CancellationToken cancellation);
}