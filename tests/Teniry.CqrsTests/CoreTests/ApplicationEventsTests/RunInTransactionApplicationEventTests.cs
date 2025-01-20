using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.ApplicationEvents;
using Teniry.Cqrs.Commands.Transactional;
using Teniry.CqrsTests.Helpers;

namespace Teniry.CqrsTests.CoreTests.ApplicationEventsTests;

public class RunInTransactionApplicationEventTests {
    private readonly CallValidator _callValidator;
    private readonly LoggerStub<ApplicationEventDispatcher> _logger;
    private readonly ServiceCollection _services;
    private readonly UnitOfWorkStub _uow;

    public RunInTransactionApplicationEventTests() {
        _services = new();
        _callValidator = new();
        _services.AddScoped<CallValidator>(_ => _callValidator);
        _uow = new();
        _services.AddScoped<UnitOfWorkStub>(_ => _uow);
        _logger = new();
    }

    [Fact]
    public async Task Should_RunEventHandler() {
        // Arrange
        _services.AddScoped<IApplicationEventHandler<TestDataUpdatedEvent>, FirstEventHandler>();
        var dispatcher = new ApplicationEventDispatcher(_services.BuildServiceProvider(), _logger);

        // Act
        var act = async () =>
            await dispatcher.DispatchAsync(new TestDataUpdatedEvent(), new());

        // Assert
        await act.Should().NotThrowAsync();
        _callValidator.Calls.Should()
            .SatisfyRespectively(first => first.Should().Be("First call"));
        _uow.Calls.Should()
            .SatisfyRespectively(
                first => first.Should().Be("Begin transaction"),
                second => second.Should().Be("Commit transaction")
            );
    }

    [Fact]
    public async Task Should_RunAllHandlers_Even_WhenOtherHandlerThrowsException() {
        // Arrange
        _services.AddScoped<IApplicationEventHandler<TestDataUpdatedEvent>, SecondEventHandler>();
        _services.AddScoped<IApplicationEventHandler<TestDataUpdatedEvent>, FirstEventHandler>();

        var dispatcher = new ApplicationEventDispatcher(_services.BuildServiceProvider(), _logger);

        // Act
        var act = async () =>
            await dispatcher.DispatchAsync(new TestDataUpdatedEvent(), new());

        // Assert
        await act.Should().NotThrowAsync();
        _callValidator.Calls.Should()
            .SatisfyRespectively(
                // Second call should not be retried because it is not db update exception
                second => second.Should().Be("Second call"),
                first => first.Should().Be("First call")
            );

        _logger.Calls.Should()
            .HaveCount(1).And
            .SatisfyRespectively(first => first.Should().Be("Error InvalidOperationException logged"));
    }

    private class TestDataUpdatedEvent : IApplicationEvent { }

    private class FirstEventHandler(CallValidator callValidator)
        : IApplicationEventHandler<TestDataUpdatedEvent>, ITransactionalHandler<UnitOfWorkStub> {
        public Task HandleAsync(
            TestDataUpdatedEvent applicationEvent,
            CancellationToken cancellation
        ) {
            callValidator.Called("First call");

            return Task.CompletedTask;
        }
    }

    private class SecondEventHandler(CallValidator callValidator)
        : IApplicationEventHandler<TestDataUpdatedEvent>, ITransactionalHandler<UnitOfWorkStub> {
        public Task HandleAsync(
            TestDataUpdatedEvent applicationEvent,
            CancellationToken cancellation
        ) {
            callValidator.Called("Second call");

            throw new InvalidOperationException("Not valid call");
        }
    }
}