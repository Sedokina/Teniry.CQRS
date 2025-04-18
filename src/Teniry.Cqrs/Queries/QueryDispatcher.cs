using Microsoft.Extensions.DependencyInjection;

namespace Teniry.Cqrs.Queries;

public class QueryDispatcher : IQueryDispatcher {
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public Task<TQueryResult> DispatchAsync<TQuery, TQueryResult>(TQuery query, CancellationToken cancellation) {
        var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();

        return handler.HandleAsync(query, cancellation);
    }
}