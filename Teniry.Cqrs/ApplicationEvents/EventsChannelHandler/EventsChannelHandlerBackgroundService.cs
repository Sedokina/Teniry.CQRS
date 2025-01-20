using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Teniry.Cqrs.ApplicationEvents.EventsChannelHandler;

internal class EventsChannelHandlerBackgroundService : BackgroundService {
    private readonly EventsChannel _eventsChannel;
    private readonly ILogger<EventsChannelHandlerBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public EventsChannelHandlerBackgroundService(
        EventsChannel eventsChannel,
        ILogger<EventsChannelHandlerBackgroundService> logger,
        IServiceProvider serviceProvider
    ) {
        _eventsChannel = eventsChannel;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken
    ) {
        try {
            do {
                try {
                    await DispatchCommandEventHandler(stoppingToken);
                } catch (Exception ex) {
                    _logger.LogError(ex, "Failed to process eventChannel's event {message}", ex.Message);
                }
            } while (!stoppingToken.IsCancellationRequested);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to process event's channel {message}", ex.Message);
        }
    }

    private async Task DispatchCommandEventHandler(
        CancellationToken stoppingToken
    ) {
        using var scope = _serviceProvider.CreateScope();
        var commandEvent = await _eventsChannel.EventsQueue.Reader.ReadAsync(stoppingToken);
        var commandEventDispatcher = scope.ServiceProvider.GetRequiredService<IApplicationEventDispatcher>();
        await commandEventDispatcher.DispatchAsync(commandEvent, stoppingToken).ConfigureAwait(false);
    }
}