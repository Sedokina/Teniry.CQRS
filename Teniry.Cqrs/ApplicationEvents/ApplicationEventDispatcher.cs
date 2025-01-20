using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Teniry.Cqrs.Commands.Transactional;
using Teniry.Cqrs.OperationRetries;

namespace Teniry.Cqrs.ApplicationEvents;

public class ApplicationEventDispatcher : IApplicationEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApplicationEventDispatcher> _logger;

    public ApplicationEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<ApplicationEventDispatcher> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync<TApplicationEvent>(
        TApplicationEvent applicationEvent,
        CancellationToken cancellation
    )
        where TApplicationEvent : IApplicationEvent
    {
        var handlerType = typeof(IApplicationEventHandler<>).MakeGenericType(applicationEvent.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            try
            {
                var applicationEventHandler = (IApplicationEventHandler<TApplicationEvent>)handler!;
                if (handler is ITransactionalHandler)
                {
                    var uow = TransactionalHandlerUnitOfWorkAccessor.GetUnitOfWork(handler, _serviceProvider);
                    var eventHandler = new ApplicationEventTransactionalHandlerProxy<TApplicationEvent>(
                        applicationEventHandler,
                        uow);

                    await RetryAsync(applicationEvent, eventHandler, eventHandler, cancellation)
                        .ConfigureAwait(false);
                    return;
                }

                if (handler is IRetriableOperation repeatableOperation)
                {
                    await RetryAsync(applicationEvent, applicationEventHandler, repeatableOperation, cancellation)
                        .ConfigureAwait(false);
                    return;
                }

                await applicationEventHandler.HandleAsync(applicationEvent, cancellation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle event with {@handler}", handler?.GetType().Name);
            }
        }
    }

    private static async Task RetryAsync<TApplicationEvent>(TApplicationEvent command,
        IApplicationEventHandler<TApplicationEvent> handler,
        IRetriableOperation repeatableOperation,
        CancellationToken cancellation)
        where TApplicationEvent : IApplicationEvent
    {
        var actionToRetry = async () => { await handler.HandleAsync(command, cancellation).ConfigureAwait(false); };
        await OperationRetry
            .RetryOnFailAsync(actionToRetry, repeatableOperation)
            .ConfigureAwait(false);
    }
}