using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.ApplicationEvents;
using Teniry.CqrsTests.Helpers;

namespace Teniry.CqrsTests.CoreTests.ApplicationEventsTests;

public class RunWithoutTransactionWithoutRetryApplicationEventTests {
    private readonly CallValidator _callValidator;
    private readonly LoggerStub<ApplicationEventDispatcher> _logger;
    private readonly ServiceCollection _services;

    public RunWithoutTransactionWithoutRetryApplicationEventTests() {
        _services = new();
        _callValidator = new();
        _services.AddScoped<CallValidator>(_ => _callValidator);
        _logger = new();
    }

    [Fact]
    public async Task Should_RunHandler() {
        // Arrange
        _services.AddScoped<IApplicationEventHandler<TestDataUpdatedEvent>, ValidEventHandler>();
        var dispatcher = new ApplicationEventDispatcher(_services.BuildServiceProvider(), _logger);

        // Act
        var act = async () =>
            await dispatcher.DispatchAsync(new TestDataUpdatedEvent(), new());

        // Assert
        await act.Should().NotThrowAsync();
        _callValidator.Calls.Should()
            .SatisfyRespectively(first => first.Should().Be("Valid call"));
    }

    [Fact]
    public async Task Should_RunHandler_When_EventCasterToIApplicationEvent() {
        // Arrange
        _services.AddScoped<IApplicationEventHandler<TestDataUpdatedEvent>, ValidEventHandler>();
        var dispatcher = new ApplicationEventDispatcher(_services.BuildServiceProvider(), _logger);

        // Act
        var act = async () =>
            await dispatcher.DispatchAsync((IApplicationEvent)new TestDataUpdatedEvent(), new());

        // Assert
        await act.Should().NotThrowAsync();
        _callValidator.Calls.Should()
            .SatisfyRespectively(first => first.Should().Be("Valid call"));
    }

    [Fact]
    public async Task Should_RunAllHandlers_Even_WhenOtherHandlerThrowsException() {
        // Arrange
        _services.AddScoped<IApplicationEventHandler<TestDataUpdatedEvent>, NotValidEventHandler>();
        _services.AddScoped<IApplicationEventHandler<TestDataUpdatedEvent>, ValidEventHandler>();
        var dispatcher = new ApplicationEventDispatcher(_services.BuildServiceProvider(), _logger);

        // Act
        var act = async () =>
            await dispatcher.DispatchAsync(new TestDataUpdatedEvent(), new());

        // Assert
        await act.Should().NotThrowAsync();
        _callValidator.Calls.Should()
            .SatisfyRespectively(
                first => first.Should().Be("Not valid call"),
                second => second.Should().Be("Valid call")
            );

        _logger.Calls.Should()
            .HaveCount(1).And
            .SatisfyRespectively(first => first.Should().Be("Error InvalidOperationException logged"));
    }

    private class TestDataUpdatedEvent : IApplicationEvent { }

    private class NotValidEventHandler(CallValidator callValidator) : IApplicationEventHandler<TestDataUpdatedEvent> {
        public Task HandleAsync(
            TestDataUpdatedEvent applicationEvent,
            CancellationToken cancellation
        ) {
            callValidator.Called("Not valid call");

            throw new InvalidOperationException("Not valid call");
        }
    }

    private class ValidEventHandler(CallValidator callValidator) : IApplicationEventHandler<TestDataUpdatedEvent> {
        public Task HandleAsync(
            TestDataUpdatedEvent applicationEvent,
            CancellationToken cancellation
        ) {
            callValidator.Called("Valid call");

            return Task.CompletedTask;
        }
    }
}