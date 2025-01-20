using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.ApplicationEvents;
using Teniry.Cqrs.Commands.Transactional;
using Teniry.Cqrs.OperationRetries;
using Teniry.CqrsTests.Helpers;

namespace Teniry.CqrsTests.CoreTests.ApplicationEventsTests;

public class RunInTransactionWithRetryApplicationEventTests {
    private readonly ServiceCollection                      _services;
    private readonly UnitOfWorkStub                              _uow;
    private readonly LoggerStub<ApplicationEventDispatcher> _logger;
    
    public RunInTransactionWithRetryApplicationEventTests() {
        _services      = new ServiceCollection();
        _uow = new UnitOfWorkStub();
        _services.AddScoped<UnitOfWorkStub>(_ => _uow);
        _logger = new LoggerStub<ApplicationEventDispatcher>();
    }
    
    [Fact]
    public async Task Should_RetryHandle_When_DbExceptionThrown() {
        // Arrange
        _services.AddScoped<IApplicationEventHandler<TestDataUpdatedEvent>, UpdateTestDataHandler>();
        var dispatcher = new ApplicationEventDispatcher(_services.BuildServiceProvider(), _logger);

        // Act
        var exception = new InvalidOperationException("", new DbUpdateException());
        await dispatcher
            .DispatchAsync(new TestDataUpdatedEvent(4, exception), new());

        // Assert
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
                tenth => tenth.Should().Be("Commit transaction"));
    }
    
    [Fact]
    public async Task Should_LogThrowException_When_RetryAttemptsExceeded() {
        // Arrange
        _services.AddScoped<IApplicationEventHandler<TestDataUpdatedEvent>, UpdateTestDataHandler>();
        var dispatcher = new ApplicationEventDispatcher(_services.BuildServiceProvider(), _logger);

        // Act
        var exception = new InvalidOperationException("", new DbUpdateException());
        var act = async () => await dispatcher
            .DispatchAsync(new TestDataUpdatedEvent(5, exception), new());

        // Assert
        await act.Should().NotThrowAsync<InvalidOperationException>();
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
                ninth => ninth.Should().Be("Begin transaction"));

        _logger.Calls.Should()
            .SatisfyRespectively(first => first.Should().Be("Error InvalidOperationException logged"));
    }

    [Fact]
    public async Task Should_RetryCustomNumberOfTimes_When_CustomNumberIsSet() {
        // Arrange
        _services.AddScoped<IApplicationEventHandler<CustomRetryEvent>, CustomRetryEventHandler>();
        var dispatcher = new ApplicationEventDispatcher(_services.BuildServiceProvider(), _logger);

        // Act
        var exception = new TestRetryException();
        await dispatcher
            .DispatchAsync(new CustomRetryEvent(4, exception), new());

        // Assert
        _uow.Calls.Should()
            .HaveCount(5)
            .And.SatisfyRespectively(
                first => first.Should().Be("Begin transaction"),
                second => second.Should().Be("Clear changes"),
                third => third.Should().Be("Begin transaction"),
                fourth => fourth.Should().Be("Clear changes"),
                fifth => fifth.Should().Be("Begin transaction"));
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
            .And.SatisfyRespectively(
                first => first.Should().Be("Begin transaction"));
    }
    
    private class TestDataUpdatedEvent(int timesToFail, Exception throwOnFail) : IApplicationEvent {
        public int       TimesToFail { get; set; } = timesToFail;
        public Exception ThrowOnFail { get; }      = throwOnFail;
    }

    private class UpdateTestDataHandler
        : IApplicationEventHandler<TestDataUpdatedEvent>, ITransactionalHandler<UnitOfWorkStub> {
        private int _timesFailed = 0;

        public Task HandleAsync(
            TestDataUpdatedEvent applicationEvent,
            CancellationToken    cancellation
        ) {
            if (_timesFailed == applicationEvent.TimesToFail) {
                return Task.FromResult("Handler called");
            }

            _timesFailed++;

            throw applicationEvent.ThrowOnFail;
        }
    }
    
    private class CustomRetryEvent(int timesToFail, Exception throwOnFail) : IApplicationEvent {
        public int       TimesToFail { get; set; } = timesToFail;
        public Exception ThrowOnFail { get; }      = throwOnFail;
    }

    private class CustomRetryEventHandler
        : IApplicationEventHandler<CustomRetryEvent>, ITransactionalHandler<UnitOfWorkStub>, IRetriableOperation {
        private int _timesFailed = 0;

        public Task HandleAsync(
            CustomRetryEvent applicationEvent,
            CancellationToken    cancellation
        ) {
            if (_timesFailed == applicationEvent.TimesToFail) {
                return Task.FromResult("Handler called");
            }

            _timesFailed++;

            throw applicationEvent.ThrowOnFail;
        }

        public int GetMaxRetryAttempts()
        {
            return 3;
        }
        
        public bool RetryOnException(Exception ex)
        {
            return ex is TestRetryException;
        }
    }
    
    private class TestRetryException : Exception
    {
    }
}