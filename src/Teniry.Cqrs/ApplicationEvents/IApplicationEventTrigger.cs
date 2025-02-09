namespace Teniry.Cqrs.ApplicationEvents;

public interface IApplicationEventTrigger {
    event ApplicationEventSubscriber ApplicationEvent;
}

public delegate Task ApplicationEventSubscriber(object sender, IApplicationEvent e, CancellationToken cancellation);