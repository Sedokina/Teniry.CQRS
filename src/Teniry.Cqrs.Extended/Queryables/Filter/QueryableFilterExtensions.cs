namespace Teniry.Cqrs.Extended.Queryables.Filter;

public static class QueryableFilterExtensions {
    public static IQueryable<TSource> Filter<TSource>(
        this IQueryable<TSource> source,
        QueryableFilter<TSource> filter
    ) {
        source = filter.ApplyFilter(source);
        source = filter.ApplySort(source);

        return source;
    }
}