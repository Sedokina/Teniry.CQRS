using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.Commands;
using Teniry.Cqrs.Commands.Transactional;
using Teniry.Cqrs.OperationRetries;
using Teniry.CqrsTests.Helpers;

namespace Teniry.CqrsTests.CoreTests.CommandTests.HasReturnValue;

public class RunInTransactionCommandWithReturnValueTests {
    private readonly ServiceCollection _services;
    private readonly UnitOfWorkStub _uow;

    public RunInTransactionCommandWithReturnValueTests() {
        _services = new();
        _uow = new();
        _services.AddScoped<UnitOfWorkStub>(_ => _uow);
    }

    [Fact]
    public async Task Should_ThrowException_When_TransactionalHasNoUnitOfWork() {
        // Arrange
        _services.AddScoped<ICommandHandler<NoUowUpdateTestDataCommand, string>, NoUowUpdateTestDataHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider(), new());

        // Act
        var act = async () =>
            await dispatcher.DispatchAsync<NoUowUpdateTestDataCommand, string>(new(), new());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>("because generic interface should be used");
    }

    [Fact]
    public async Task Should_CallHandlerInTransaction() {
        // Arrange
        _services.AddScoped<ICommandHandler<UpdateTestDataCommand, string>, UpdateTestDataHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider(), new());

        // Act
        var result = await dispatcher
            .DispatchAsync<UpdateTestDataCommand, string>(new(), new());

        // Assert
        result.Should().Be("Handler called");
        _uow.Calls.Should()
            .SatisfyRespectively(
                first => first.Should().Be("Begin transaction"),
                second => second.Should().Be("Commit transaction")
            );
    }

    private class NoUowUpdateTestDataCommand { }

    private class NoUowUpdateTestDataHandler
        : ICommandHandler<NoUowUpdateTestDataCommand, string>,
            IRetriableOperation,
            ITransactionalHandler {
        public Task<string> HandleAsync(NoUowUpdateTestDataCommand command, CancellationToken cancellation) {
            return Task.FromResult("Handler called");
        }
    }

    private class UpdateTestDataCommand { }

    private class UpdateTestDataHandler
        : ICommandHandler<UpdateTestDataCommand, string>, ITransactionalHandler<UnitOfWorkStub> {
        public Task<string> HandleAsync(UpdateTestDataCommand command, CancellationToken cancellation) {
            return Task.FromResult("Handler called");
        }
    }
}