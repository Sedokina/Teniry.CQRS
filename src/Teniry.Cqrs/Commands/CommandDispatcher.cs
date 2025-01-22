using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.ApplicationEvents;
using Teniry.Cqrs.ApplicationEvents.EventsChannelHandler;
using Teniry.Cqrs.Commands.Transactional;
using Teniry.Cqrs.OperationRetries;

namespace Teniry.Cqrs.Commands;

public class CommandDispatcher : ICommandDispatcher {
    private readonly IServiceProvider _serviceProvider;
    private readonly EventsChannel _eventsChannel;

    public CommandDispatcher(
        IServiceProvider serviceProvider,
        EventsChannel eventsChannel
    ) {
        _serviceProvider = serviceProvider;
        _eventsChannel = eventsChannel;
    }

    public async Task DispatchAsync<TCommand>(
        TCommand command,
        CancellationToken cancellation
    ) {
        await ValidateCommandAsync(command, cancellation);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        var eventTriggerHandler = handler as IApplicationEventTrigger;
        if (eventTriggerHandler is not null) {
            eventTriggerHandler.ApplicationEvent += DispatchHandlerEvents;
        }

        await RunHandlerAsync(command, handler, cancellation).ConfigureAwait(false);

        if (eventTriggerHandler is not null) {
            eventTriggerHandler.ApplicationEvent -= DispatchHandlerEvents;
        }
    }

    public async Task<TCommandResult> DispatchAsync<TCommand, TCommandResult>(
        TCommand command,
        CancellationToken cancellation
    ) {
        await ValidateCommandAsync(command, cancellation);

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TCommandResult>>();

        var eventTriggerHandler = handler as IApplicationEventTrigger;
        if (eventTriggerHandler is not null) {
            eventTriggerHandler.ApplicationEvent += DispatchHandlerEvents;
        }

        var result = await RunHandlerWithReturnValueAsync(command, handler, cancellation).ConfigureAwait(false);

        if (eventTriggerHandler is not null) {
            eventTriggerHandler.ApplicationEvent -= DispatchHandlerEvents;
        }

        return result;
    }

    private async Task RunHandlerAsync<TCommand>(
        TCommand command,
        ICommandHandler<TCommand> handler,
        CancellationToken cancellation
    ) {
        if (handler is ITransactionalHandler) {
            var uow = TransactionalHandlerUnitOfWorkAccessor.GetUnitOfWork(handler, _serviceProvider);
            var commandHandler = new CommandTransactionalHandlerProxy<TCommand>(handler, uow);

            await RetryAsync(command, commandHandler, commandHandler, cancellation).ConfigureAwait(false);

            return;
        }

        if (handler is IRetriableOperation repeatableOperation) {
            await RetryAsync(command, handler, repeatableOperation, cancellation).ConfigureAwait(false);

            return;
        }

        await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
    }

    private async Task<TCommandResult> RunHandlerWithReturnValueAsync<TCommand, TCommandResult>(
        TCommand command,
        ICommandHandler<TCommand, TCommandResult> handler,
        CancellationToken cancellation
    ) {
        if (handler is ITransactionalHandler) {
            var uow = TransactionalHandlerUnitOfWorkAccessor.GetUnitOfWork(handler, _serviceProvider);
            var commandHandler = new CommandTransactionalHandlerProxy<TCommand, TCommandResult>(handler, uow);

            return await RetryAsync(command, commandHandler, commandHandler, cancellation).ConfigureAwait(false);
        }

        if (handler is IRetriableOperation repeatableOperation) {
            return await RetryAsync(command, handler, repeatableOperation, cancellation).ConfigureAwait(false);
        }

        return await handler.HandleAsync(command, cancellation).ConfigureAwait(false);
    }

    private async Task ValidateCommandAsync<TCommand>(
        TCommand command,
        CancellationToken cancellation
    ) {
        var validator = _serviceProvider.GetService<IValidator<TCommand>>();

        if (validator is not null) {
            await validator.ValidateAndThrowAsync(command, cancellation);
        }
    }

    private static async Task<TCommandResult> RetryAsync<TCommand, TCommandResult>(
        TCommand command,
        ICommandHandler<TCommand, TCommandResult> handler,
        IRetriableOperation repeatableOperation,
        CancellationToken cancellation
    ) {
        var actionToRetry = async () => await handler.HandleAsync(command, cancellation).ConfigureAwait(false);

        return await OperationRetry
            .RetryOnFailAsync(actionToRetry, repeatableOperation)
            .ConfigureAwait(false);
    }

    private static async Task RetryAsync<TCommand>(
        TCommand command,
        ICommandHandler<TCommand> handler,
        IRetriableOperation repeatableOperation,
        CancellationToken cancellation
    ) {
        var actionToRetry = async () => { await handler.HandleAsync(command, cancellation).ConfigureAwait(false); };

        await OperationRetry
            .RetryOnFailAsync(actionToRetry, repeatableOperation)
            .ConfigureAwait(false);
    }

    private async Task DispatchHandlerEvents(
        object sender,
        IApplicationEvent applicationEvent,
        CancellationToken cancellation
    ) {
        await _eventsChannel.EventsQueue.Writer.WriteAsync(applicationEvent, cancellation);
    }
}