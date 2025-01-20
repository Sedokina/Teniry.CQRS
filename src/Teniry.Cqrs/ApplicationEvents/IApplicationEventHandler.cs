namespace Teniry.Cqrs.ApplicationEvents;

public interface IApplicationEventHandler<in TApplicationEvent> where TApplicationEvent : IApplicationEvent {
    public Task HandleAsync(TApplicationEvent applicationEvent, CancellationToken cancellation);
}