using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.Commands;
using Teniry.Cqrs.Commands.Transactional;
using Teniry.CqrsTests.Helpers;

namespace Teniry.CqrsTests.CoreTests.CommandTests.NoReturnValue;

public class RunInTransactionCommandTests {
    private readonly ServiceCollection _services;
    private readonly UnitOfWorkStub _uow;

    public RunInTransactionCommandTests() {
        _services = new();
        _uow = new();
        _services.AddScoped<UnitOfWorkStub>(_ => _uow);
    }

    [Fact]
    public async Task Should_ThrowException_When_TransactionalHasNoUnitOfWork() {
        // Arrange
        _services.AddScoped<ICommandHandler<NoUowUpdateTestDataCommand>, NoUowUpdateTestDataHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider(), new());

        // Act
        var act = async () =>
            await dispatcher.DispatchAsync(new NoUowUpdateTestDataCommand(), new());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>("because generic interface should be used");
    }

    [Fact]
    public async Task Should_CallHandlerInTransaction() {
        // Arrange
        _services.AddScoped<ICommandHandler<UpdateTestDataCommand>, UpdateTestDataHandler>();

        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider(), new());

        // Act
        var act = async () =>
            await dispatcher.DispatchAsync(new UpdateTestDataCommand(), new());

        // Assert
        await act.Should().NotThrowAsync();
        _uow.Calls.Should()
            .SatisfyRespectively(
                first => first.Should().Be("Begin transaction"),
                second => second.Should().Be("Commit transaction")
            );
    }

    private class NoUowUpdateTestDataCommand { }

    private class NoUowUpdateTestDataHandler : ICommandHandler<NoUowUpdateTestDataCommand>, ITransactionalHandler {
        public Task HandleAsync(
            NoUowUpdateTestDataCommand command,
            CancellationToken cancellation
        ) {
            return Task.CompletedTask;
        }
    }

    private class UpdateTestDataCommand { }

    private class
        UpdateTestDataHandler : ICommandHandler<UpdateTestDataCommand>, ITransactionalHandler<UnitOfWorkStub> {
        public Task HandleAsync(
            UpdateTestDataCommand command,
            CancellationToken cancellation
        ) {
            return Task.CompletedTask;
        }
    }
}