using X.PagedList;

namespace Teniry.Cqrs.Queryables.Page;

public static class PageInfoExtensions {
    public static PageInfo GetPage<T>(this IPagedList<T> pagedList) {
        return new(
            pagedList.PageNumber,
            pagedList.PageSize,
            pagedList.TotalItemCount,
            pagedList.PageCount,
            pagedList.HasPreviousPage,
            pagedList.HasNextPage,
            pagedList.IsFirstPage,
            pagedList.IsLastPage,
            pagedList.FirstItemOnPage,
            pagedList.LastItemOnPage
        );
    }
}