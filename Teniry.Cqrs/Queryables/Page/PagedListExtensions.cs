using X.PagedList;

namespace Teniry.Cqrs.Queryables.Page;

public static class PagedListExtensions {
    public static async Task<IPagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        IPage              page,
        CancellationToken  cancellationToken
    ) {
        return await query.ToPagedListAsync(page.Page, page.PageSize, null, cancellationToken);
    }
}