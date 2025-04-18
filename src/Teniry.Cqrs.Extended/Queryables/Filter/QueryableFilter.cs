using System.Linq.Expressions;
using Teniry.Cqrs.Extended.Queryables.Sort;

namespace Teniry.Cqrs.Extended.Queryables.Filter;

public abstract class QueryableFilter<TEntity> {
    public string[]? Sort { get; set; }

    public IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query) {
        return Filter(query);
    }

    public IQueryable<TEntity> ApplySort(IQueryable<TEntity> query) {
        if (Sort is null || !Sort.Any()) return DefaultSort(query);

        var availableSorts = DefineSort();
        IOrderedQueryable<TEntity> ordered = null!;

        foreach (var sortKey in Sort) {
            if (!SortKey.TryParse(sortKey, out var orderProperty)) continue;

            if (!availableSorts.TryGetValue(orderProperty.Property, out var orderByFunc)) continue;

            if (orderProperty.Direction == SortDirection.Asc) {
                ordered = ordered == null ?
                    query.OrderBy(orderByFunc) :
                    ordered.ThenBy(orderByFunc);
            } else {
                ordered = ordered == null ?
                    query.OrderByDescending(orderByFunc) :
                    ordered.ThenByDescending(orderByFunc);
            }
        }

        return ordered ?? query;
    }

    public abstract Dictionary<string, Expression<Func<TEntity, object>>> DefineSort();

    protected virtual IQueryable<TEntity> DefaultSort(IQueryable<TEntity> query) {
        return query;
    }

    protected abstract IQueryable<TEntity> Filter(IQueryable<TEntity> query);
}