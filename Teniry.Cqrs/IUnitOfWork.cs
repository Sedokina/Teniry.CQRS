using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Teniry.Cqrs;

public interface IUnitOfWork {
    /// <summary>
    ///     Same as Database.BeginTransactionAsync
    /// </summary>
    /// <remarks>
    ///     This method is virtual and wraps Database.BeginTransaction so that begin transaction can be overwritten
    /// </remarks>
    /// <param name="isolationLevel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel    isolationLevel,
        CancellationToken cancellationToken = default
    );

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    void ClearChanges();
}