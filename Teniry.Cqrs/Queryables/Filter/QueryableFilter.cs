using System.Linq.Expressions;
using Teniry.Cqrs.Queryables.Sort;

namespace Teniry.Cqrs.Queryables.Filter;

public abstract class QueryableFilter<TEntity> {
    public string[]? Sorts { get; set; }

    public IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query) {
        return Filter(query);
    }

    public IQueryable<TEntity> ApplySort(IQueryable<TEntity> query) {
        if (Sorts is null || !Sorts.Any()) return DefaultSort(query);

        var                        availableSorts = Sort();
        IOrderedQueryable<TEntity> ordered        = null!;

        foreach (var sortKey in Sorts)
        {
            if (!SortKey.TryParse(sortKey, out var orderProperty)) continue;

            if (!availableSorts.TryGetValue(orderProperty.Property, out var orderByFunc)) continue;

            if (orderProperty.Direction == SortDirection.Asc)
            {
                ordered = ordered == null ? 
                    query.OrderBy(orderByFunc) :
                    ordered.ThenBy(orderByFunc);
            }
            else
            {
                ordered = ordered == null ?
                    query.OrderByDescending(orderByFunc) :
                    ordered.ThenByDescending(orderByFunc);
            }
        }

        return ordered ?? query;
    }

    public abstract Dictionary<string, Expression<Func<TEntity, object>>> Sort();

    protected virtual IQueryable<TEntity> DefaultSort(IQueryable<TEntity> query) {
        return query;
    }

    protected abstract IQueryable<TEntity> Filter(IQueryable<TEntity> query);
}