using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Teniry.Cqrs.Commands.Transactional;
using Teniry.Cqrs.OperationRetries;

namespace Teniry.Cqrs.ApplicationEvents;

public class ApplicationEventDispatcher : IApplicationEventDispatcher {
    private readonly ILogger<ApplicationEventDispatcher> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ApplicationEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<ApplicationEventDispatcher> logger
    ) {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync<TApplicationEvent>(
        TApplicationEvent applicationEvent,
        CancellationToken cancellation
    )
        where TApplicationEvent : IApplicationEvent {
        var handlerType = typeof(IApplicationEventHandler<>).MakeGenericType(applicationEvent.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers) {
            try {
                var methodInfo = handler!
                    .GetType()
                    .GetMethod(nameof(IApplicationEventHandler<TApplicationEvent>.HandleAsync))!;

                if (handler is ITransactionalHandler) {
                    var uow = TransactionalHandlerUnitOfWorkAccessor.GetUnitOfWork(handler, _serviceProvider);
                    var eventHandler = new ApplicationEventTransactionalHandlerProxy<TApplicationEvent>(
                        handler,
                        methodInfo,
                        uow
                    );

                    var actionToRetry = async () => {
                        await eventHandler.HandleAsync(applicationEvent, cancellation).ConfigureAwait(false);
                    };
                    await OperationRetry
                        .RetryOnFailAsync(actionToRetry, eventHandler)
                        .ConfigureAwait(false);

                    return;
                }

                if (handler is IRetriableOperation repeatableOperation) {
                    await RetryAsync(applicationEvent, handler, methodInfo, repeatableOperation, cancellation)
                        .ConfigureAwait(false);

                    return;
                }

                await InvokeHandlerAsync(applicationEvent, cancellation, methodInfo, handler).ConfigureAwait(false);
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to handle event with {@handler}", handler?.GetType().Name);
            }
        }
    }

    private static async Task RetryAsync<TApplicationEvent>(
        TApplicationEvent applicationEvent,
        object handler,
        MethodInfo methodInfo,
        IRetriableOperation repeatableOperation,
        CancellationToken cancellation
    )
        where TApplicationEvent : IApplicationEvent {
        var actionToRetry = async () => {
            await InvokeHandlerAsync(applicationEvent, cancellation, methodInfo, handler).ConfigureAwait(false);
        };
        await OperationRetry
            .RetryOnFailAsync(actionToRetry, repeatableOperation)
            .ConfigureAwait(false);
    }

    private static async Task InvokeHandlerAsync<TApplicationEvent>(
        TApplicationEvent applicationEvent,
        CancellationToken cancellation,
        MethodInfo? methodInfo,
        object handler
    ) where TApplicationEvent : IApplicationEvent {
        try {
            var handlerAsyncTask = (Task)methodInfo!.Invoke(handler, [applicationEvent, cancellation])!;
            await handlerAsyncTask.ConfigureAwait(false);
        } catch (TargetInvocationException ex) {
            // Target TargetInvocationException is thrown because handler is invoked via Invoke method
            // If any exception occurs inside handler it is wrapped into TargetInvocationException
            // This line unwraps initial exception occured in the handler and throws that exception
            ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
        }
    }
}