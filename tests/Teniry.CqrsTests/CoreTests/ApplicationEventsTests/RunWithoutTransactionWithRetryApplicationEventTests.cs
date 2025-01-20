using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.ApplicationEvents;
using Teniry.Cqrs.Commands.Transactional;
using Teniry.Cqrs.OperationRetries;
using Teniry.CqrsTests.Helpers;

namespace Teniry.CqrsTests.CoreTests.ApplicationEventsTests;

public class RunWithoutTransactionWithRetryApplicationEventTests {
    private readonly LoggerStub<ApplicationEventDispatcher> _logger;
    private readonly ServiceCollection _services;
    private readonly UnitOfWorkStub _uow;

    public RunWithoutTransactionWithRetryApplicationEventTests() {
        _services = new();
        _uow = new();
        _services.AddScoped<UnitOfWorkStub>(_ => _uow);
        _logger = new();
    }

    [Fact]
    public async Task Should_RetryHandle_When_SpecifiedExceptionThrown() {
        // Arrange
        _services.AddScoped<IApplicationEventHandler<CustomRetryEvent>, CustomRetryEventHandler>();
        var dispatcher = new ApplicationEventDispatcher(_services.BuildServiceProvider(), _logger);

        // Act
        var exception = new TestRetryException();
        await dispatcher.DispatchAsync(new CustomRetryEvent(2, exception), new());

        // Assert
        _uow.Calls.Should()
            .SatisfyRespectively(
                first => first.Should().Be("Begin transaction"),
                second => second.Should().Be("Clear changes"),
                third => third.Should().Be("Begin transaction"),
                fourth => fourth.Should().Be("Clear changes"),
                fifth => fifth.Should().Be("Begin transaction"),
                sixth => sixth.Should().Be("Commit transaction")
            );
    }

    [Fact]
    public async Task Should_LogThrowException_When_RetryAttemptsExceeded() {
        // Arrange
        _services.AddScoped<IApplicationEventHandler<CustomRetryEvent>, CustomRetryEventHandler>();
        var dispatcher = new ApplicationEventDispatcher(_services.BuildServiceProvider(), _logger);

        // Act
        var exception = new TestRetryException();
        var act = async () => await dispatcher.DispatchAsync(new CustomRetryEvent(4, exception), new());

        // Assert
        await act.Should().NotThrowAsync<InvalidOperationException>();
        _uow.Calls.Should()
            .SatisfyRespectively(
                first => first.Should().Be("Begin transaction"),
                second => second.Should().Be("Clear changes"),
                third => third.Should().Be("Begin transaction"),
                fourth => fourth.Should().Be("Clear changes"),
                fifth => fifth.Should().Be("Begin transaction")
            );

        _logger.Calls.Should()
            .SatisfyRespectively(first => first.Should().Be("Error TestRetryException logged"));
    }

    [Fact]
    public async Task Should_NotRetryOnAnyException_Except_CustomExceptionSetInHandler() {
        // Arrange
        _services.AddScoped<IApplicationEventHandler<CustomRetryEvent>, CustomRetryEventHandler>();
        var dispatcher = new ApplicationEventDispatcher(_services.BuildServiceProvider(), _logger);

        // Act
        var exception = new Exception();
        await dispatcher
            .DispatchAsync(new CustomRetryEvent(5, exception), new());

        // Assert
        _uow.Calls.Should()
            .HaveCount(1)
            .And.SatisfyRespectively(first => first.Should().Be("Begin transaction"));
    }

    private class CustomRetryEvent(int timesToFail, Exception throwOnFail) : IApplicationEvent {
        public int TimesToFail { get; } = timesToFail;
        public Exception ThrowOnFail { get; } = throwOnFail;
    }

    private class CustomRetryEventHandler
        : IApplicationEventHandler<CustomRetryEvent>, ITransactionalHandler<UnitOfWorkStub>, IRetriableOperation {
        private int _timesFailed;

        public Task HandleAsync(
            CustomRetryEvent applicationEvent,
            CancellationToken cancellation
        ) {
            if (_timesFailed == applicationEvent.TimesToFail) {
                return Task.FromResult("Handler called");
            }

            _timesFailed++;

            throw applicationEvent.ThrowOnFail;
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