using Microsoft.EntityFrameworkCore;
using Teniry.Cqrs.OperationRetries;

namespace Teniry.Cqrs.Commands.Transactional;

public interface ITransactionalHandler {
}

/// <summary>
///     - Wraps command handler into a database transaction before it's been called <br />
///     - Always set isolation level for a transaction to IsolationLevel.RepeatableRead <br />
///     - Automatically commits transaction after handler finished successfully <br />
///     - By default repeats 5 times <br />
/// </summary>
/// <remarks>
///     Transactional handler is retriable by default only for <see cref="DbUpdateException"/>
///     which can be caused by simultaneous data update in the db
/// </remarks>
public interface ITransactionalHandler<T> : ITransactionalHandler, IRetriableOperation
    where T : IUnitOfWork
{
    bool IRetriableOperation.RetryOnException(Exception ex)
    {
        return ex is InvalidOperationException && ex.InnerException is DbUpdateException || ex is DbUpdateException;
    }
}