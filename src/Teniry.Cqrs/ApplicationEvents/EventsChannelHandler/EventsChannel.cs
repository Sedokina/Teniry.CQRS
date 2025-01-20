using System.Threading.Channels;

namespace Teniry.Cqrs.ApplicationEvents.EventsChannelHandler;

internal class EventsChannel {
    public Channel<IApplicationEvent> EventsQueue { get; set; }

    public EventsChannel() {
        EventsQueue = Channel.CreateUnbounded<IApplicationEvent>();
    }
}