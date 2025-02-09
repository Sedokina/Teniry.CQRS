using System.Data;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.EntityFrameworkCore;
using Teniry.Cqrs.OperationRetries;

namespace Teniry.Cqrs.ApplicationEvents;

internal class ApplicationEventTransactionalHandlerProxy<TApplicationEvent>
    : IApplicationEventHandler<TApplicationEvent>, IRetriableOperation where TApplicationEvent : IApplicationEvent {
    private readonly object _handler;
    private readonly MethodInfo _handlerMethodInfo;
    private readonly IUnitOfWork _uow;

    public ApplicationEventTransactionalHandlerProxy(
        object handler,
        MethodInfo handlerMethodInfo,
        IUnitOfWork uow
    ) {
        _handler = handler;
        _handlerMethodInfo = handlerMethodInfo;
        _uow = uow;
    }

    public async Task HandleAsync(
        TApplicationEvent applicationEvent,
        CancellationToken cancellation
    ) {
        // Beware:
        // Isolation level of the transaction is set to RepeatableRead to forbid entity changes concurrently
        // If during the execution of this transaction, another user updated same entity
        // this transaction would throw exception on commit to indicate concurrent data update exception
        await using (await _uow.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellation)) {
            await InvokeHandlerAsync(applicationEvent, _handler, _handlerMethodInfo, cancellation)
                .ConfigureAwait(false);
            await _uow.CommitTransactionAsync(cancellation).ConfigureAwait(false);
        }
    }

    public int GetMaxRetryAttempts() {
        if (_handler is IRetriableOperation retriableOperation) {
            return retriableOperation.GetMaxRetryAttempts();
        }

        return IRetriableOperation.DefaultRetryAttempts;
    }

    public ValueTask CleanupBeforeRetryAsync() {
        _uow.ClearChanges();

        return ValueTask.CompletedTask;
    }

    public bool RetryOnException(Exception ex) {
        if (_handler is IRetriableOperation retriableOperation) {
            return retriableOperation.RetryOnException(ex) ||
                ex is InvalidOperationException && ex.InnerException is DbUpdateException ||
                ex is DbUpdateException;
        }

        return ex is InvalidOperationException && ex.InnerException is DbUpdateException || ex is DbUpdateException;
    }

    private static async Task InvokeHandlerAsync<TApplicationEvent>(
        TApplicationEvent applicationEvent,
        object handler,
        MethodInfo? methodInfo,
        CancellationToken cancellation
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