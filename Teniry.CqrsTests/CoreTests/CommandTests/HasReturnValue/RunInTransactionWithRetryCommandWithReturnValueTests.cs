using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.Commands;
using Teniry.Cqrs.Commands.Transactional;
using Teniry.Cqrs.OperationRetries;
using Teniry.CqrsTests.Helpers;

namespace Teniry.CqrsTests.CoreTests.CommandTests.HasReturnValue;

public class RunInTransactionWithRetryCommandWithReturnValueTests {
    private readonly ServiceCollection _services;
    private readonly UnitOfWorkStub _uow;

    public RunInTransactionWithRetryCommandWithReturnValueTests() {
        _services = new();
        _uow = new();
        _services.AddScoped<UnitOfWorkStub>(_ => _uow);
    }

    [Fact]
    public async Task Should_RetryHandle_When_DbExceptionThrown() {
        // Arrange
        _services.AddScoped<ICommandHandler<UpdateTestDataCommand, string>, UpdateTestDataHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider());

        // Act
        var exception = new InvalidOperationException("", new DbUpdateException());
        var result = await dispatcher
            .DispatchAsync<UpdateTestDataCommand, string>(new(4, exception), new());

        // Assert
        result.Should().Be("Handler called");
        _uow.Calls.Should()
            .SatisfyRespectively(
                first => first.Should().Be("Begin transaction"),
                second => second.Should().Be("Clear changes"),
                third => third.Should().Be("Begin transaction"),
                fourth => fourth.Should().Be("Clear changes"),
                fifth => fifth.Should().Be("Begin transaction"),
                sixth => sixth.Should().Be("Clear changes"),
                seventh => seventh.Should().Be("Begin transaction"),
                eighth => eighth.Should().Be("Clear changes"),
                ninth => ninth.Should().Be("Begin transaction"),
                tenth => tenth.Should().Be("Commit transaction")
            );
    }

    [Fact]
    public async Task Should_ThrowException_When_RetryAttemptsExceeded() {
        // Arrange
        _services.AddScoped<ICommandHandler<UpdateTestDataCommand, string>, UpdateTestDataHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider());

        // Act
        var exception = new InvalidOperationException("", new DbUpdateException());
        var act = async () => await dispatcher
            .DispatchAsync<UpdateTestDataCommand, string>(new(5, exception), new());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _uow.Calls.Should()
            .SatisfyRespectively(
                first => first.Should().Be("Begin transaction"),
                second => second.Should().Be("Clear changes"),
                third => third.Should().Be("Begin transaction"),
                fourth => fourth.Should().Be("Clear changes"),
                fifth => fifth.Should().Be("Begin transaction"),
                sixth => sixth.Should().Be("Clear changes"),
                seventh => seventh.Should().Be("Begin transaction"),
                eighth => eighth.Should().Be("Clear changes"),
                ninth => ninth.Should().Be("Begin transaction")
            );
    }

    [Fact]
    public async Task Should_NotRetry_When_NotDbUpdateExceptionThrown() {
        // Arrange
        _services.AddScoped<ICommandHandler<UpdateTestDataCommand, string>, UpdateTestDataHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider());

        // Act
        var exception = new InvalidOperationException();
        var act = async () => await dispatcher
            .DispatchAsync<UpdateTestDataCommand, string>(new(4, exception), new());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _uow.Calls.Should()
            .SatisfyRespectively(first => first.Should().Be("Begin transaction"));
    }

    [Fact]
    public async Task Should_RetryCustomNumberOfTimes_When_CustomNumberIsSet() {
        // Arrange
        _services.AddScoped<ICommandHandler<CustomRetryCommand, string>, CustomRetryHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider());

        // Act
        var exception = new TestRetryException();
        var act = async () =>
            await dispatcher.DispatchAsync<CustomRetryCommand, string>(new(5, exception), new());

        // Assert
        await act.Should().ThrowAsync<TestRetryException>();
        _uow.Calls.Should()
            .HaveCount(5)
            .And.SatisfyRespectively(
                first => first.Should().Be("Begin transaction"),
                second => second.Should().Be("Clear changes"),
                third => third.Should().Be("Begin transaction"),
                fourth => fourth.Should().Be("Clear changes"),
                fifth => fifth.Should().Be("Begin transaction")
            );
    }

    [Fact]
    public async Task Should_NotRetryOnAnyException_Except_CustomExceptionSetInHandler() {
        // Arrange
        _services.AddScoped<ICommandHandler<CustomRetryCommand, string>, CustomRetryHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider());

        // Act
        var exception = new Exception();
        var act = async () =>
            await dispatcher.DispatchAsync<CustomRetryCommand, string>(new(5, exception), new());

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _uow.Calls.Should()
            .SatisfyRespectively(first => first.Should().Be("Begin transaction"));
    }

    private class UpdateTestDataCommand(int timesToFail, Exception throwOnFail) {
        public int TimesToFail { get; } = timesToFail;
        public Exception ThrowOnFail { get; } = throwOnFail;
    }

    private class UpdateTestDataHandler
        : ICommandHandler<UpdateTestDataCommand, string>, ITransactionalHandler<UnitOfWorkStub> {
        private int _timesFailed;

        public Task<string> HandleAsync(UpdateTestDataCommand command, CancellationToken cancellation) {
            if (_timesFailed == command.TimesToFail) {
                return Task.FromResult("Handler called");
            }

            _timesFailed++;

            throw command.ThrowOnFail;
        }
    }

    private class CustomRetryCommand(int timesToFail, Exception throwOnFail) {
        public int TimesToFail { get; } = timesToFail;
        public Exception ThrowOnFail { get; } = throwOnFail;
    }

    private class CustomRetryHandler
        : ICommandHandler<CustomRetryCommand, string>,
            ITransactionalHandler<UnitOfWorkStub>,
            IRetriableOperation {
        private int _timesFailed;

        public Task<string> HandleAsync(
            CustomRetryCommand command,
            CancellationToken cancellation
        ) {
            if (_timesFailed == command.TimesToFail) {
                return Task.FromResult("Handler called");
            }

            _timesFailed++;

            throw command.ThrowOnFail;
        }

        public int GetMaxRetryAttempts() {
            return 3;
        }

        public bool RetryOnException(Exception ex) {
            return ex is TestRetryException;
        }
    }

    private class TestRetryException : Exception { }
}