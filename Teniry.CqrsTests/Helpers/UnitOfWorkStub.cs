using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Teniry.Cqrs;

namespace Teniry.CqrsTests.Helpers;

internal class UnitOfWorkStub : IUnitOfWork {
    private readonly CallValidator _callValidator = new();
    public           List<string>  Calls => _callValidator.Calls;

    public Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel    isolationLevel,
        CancellationToken cancellationToken = default
    ) {
        _callValidator.Called("Begin transaction");
        var transaction = new Mock<IDbContextTransaction>();

        return Task.FromResult(transaction.Object);
    }

    public Task CommitTransactionAsync(
        CancellationToken cancellationToken = default
    ) {
        _callValidator.Called("Commit transaction");

        return Task.CompletedTask;
    }

    public void ClearChanges() {
        _callValidator.Called("Clear changes");
    }
}