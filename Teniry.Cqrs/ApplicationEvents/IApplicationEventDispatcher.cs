namespace Teniry.Cqrs.ApplicationEvents;

public interface IApplicationEventDispatcher
{
    public Task DispatchAsync<TApplicationEvent>(TApplicationEvent applicationEvent, CancellationToken cancellation)
        where TApplicationEvent : IApplicationEvent;
}