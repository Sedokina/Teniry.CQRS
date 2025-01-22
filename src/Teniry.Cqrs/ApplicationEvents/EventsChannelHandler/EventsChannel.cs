using System.Threading.Channels;

namespace Teniry.Cqrs.ApplicationEvents.EventsChannelHandler;

public class EventsChannel {
    public Channel<IApplicationEvent> EventsQueue { get; set; }

    public EventsChannel() {
        EventsQueue = Channel.CreateUnbounded<IApplicationEvent>();
    }
}