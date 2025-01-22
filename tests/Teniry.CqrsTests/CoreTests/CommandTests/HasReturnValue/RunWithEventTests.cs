using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.ApplicationEvents;
using Teniry.Cqrs.ApplicationEvents.EventsChannelHandler;
using Teniry.Cqrs.Commands;

namespace Teniry.CqrsTests.CoreTests.CommandTests.HasReturnValue;

public class RunWithEventTests {
    private readonly ServiceCollection _services;

    public RunWithEventTests() {
        _services = new();
    }

    [Fact]
    public async Task Should_CallHandlerInTransaction() {
        // Arrange
        _services.AddScoped<ICommandHandler<UpdateTestDataCommand, string>, UpdateTestDataHandler>();
        var eventsChannel = new EventsChannel();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider(), eventsChannel);

        // Act
        var result = await dispatcher
            .DispatchAsync<UpdateTestDataCommand, string>(new(), new());

        // Assert
        result.Should().Be("Handler called");
        var readEvent = await eventsChannel.EventsQueue.Reader.ReadAsync();
        readEvent.Should().NotBeNull()
            .And.BeOfType<CustomEvent>();
    }

    private class UpdateTestDataCommand { }

    private class UpdateTestDataHandler
        : ICommandHandler<UpdateTestDataCommand, string>,
            IApplicationEventTrigger {
        /// <inheritdoc />
        public event ApplicationEventSubscriber? ApplicationEvent;

        public Task<string> HandleAsync(UpdateTestDataCommand command, CancellationToken cancellation) {
            ApplicationEvent?.Invoke(this, new CustomEvent(), cancellation);

            return Task.FromResult("Handler called");
        }
    }

    private class CustomEvent : IApplicationEvent { }
}