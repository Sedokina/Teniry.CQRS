using System.Data;
using Microsoft.EntityFrameworkCore;
using Teniry.Cqrs.OperationRetries;

namespace Teniry.Cqrs.ApplicationEvents;

internal class ApplicationEventTransactionalHandlerProxy<TApplicationEvent>
    : IApplicationEventHandler<TApplicationEvent>, IRetriableOperation where TApplicationEvent : IApplicationEvent {
    private readonly IApplicationEventHandler<TApplicationEvent> _handler;
    private readonly IUnitOfWork _uow;

    public ApplicationEventTransactionalHandlerProxy(
        IApplicationEventHandler<TApplicationEvent> handler,
        IUnitOfWork uow
    ) {
        _handler = handler;
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
            await _handler.HandleAsync(applicationEvent, cancellation).ConfigureAwait(false);
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
}