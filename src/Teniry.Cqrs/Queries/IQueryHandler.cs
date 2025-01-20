namespace Teniry.Cqrs.Queries;

public interface IQueryHandler<in TQuery, TQueryResult> {
    Task<TQueryResult> HandleAsync(TQuery query, CancellationToken cancellation);
}