using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.ApplicationEvents;
using Teniry.Cqrs.ApplicationEvents.EventsChannelHandler;
using Teniry.Cqrs.Commands;

namespace Teniry.CqrsTests.CoreTests.CommandTests.NoReturnValue;

public class RunWithEventTests {
    private readonly ServiceCollection _services;

    public RunWithEventTests() {
        _services = new();
    }

    [Fact]
    public async Task Should_CallHandlerInTransaction() {
        // Arrange
        _services.AddScoped<ICommandHandler<UpdateTestDataCommand>, UpdateTestDataHandler>();

        var eventsChannel = new EventsChannel();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider(), eventsChannel);

        // Act
        var act = async () =>
            await dispatcher.DispatchAsync(new UpdateTestDataCommand(), new());

        // Assert
        await act.Should().NotThrowAsync();
        var readEvent = await eventsChannel.EventsQueue.Reader.ReadAsync();
        readEvent.Should().NotBeNull()
            .And.BeOfType<CustomEvent>();
    }

    private class UpdateTestDataCommand { }

    private class UpdateTestDataHandler
        : ICommandHandler<UpdateTestDataCommand>,
            IApplicationEventTrigger {
        /// <inheritdoc />
        public event ApplicationEventSubscriber? ApplicationEvent;

        public Task HandleAsync(
            UpdateTestDataCommand command,
            CancellationToken cancellation
        ) {
            ApplicationEvent?.Invoke(this, new CustomEvent(), cancellation);

            return Task.CompletedTask;
        }
    }

    private class CustomEvent : IApplicationEvent { }
}