namespace Teniry.Cqrs.Queryables.Page;

public class PageInfo {
    public int CurrentPageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalItemCount { get; set; }
    public int TotalPageCount { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public bool IsFirstPage { get; set; }
    public bool IsLastPage { get; set; }
    public int StartItemIndex { get; set; }
    public int EndItemIndex { get; set; }

    public PageInfo(
        int currentPageIndex,
        int pageSize,
        int totalItemCount,
        int totalPageCount,
        bool hasPreviousPage,
        bool hasNextPage,
        bool isFirstPage,
        bool isLastPage,
        int startItemIndex,
        int endItemIndex
    ) {
        CurrentPageIndex = currentPageIndex;
        PageSize = pageSize;
        TotalItemCount = totalItemCount;
        TotalPageCount = totalPageCount;
        HasPreviousPage = hasPreviousPage;
        HasNextPage = hasNextPage;
        IsFirstPage = isFirstPage;
        IsLastPage = isLastPage;
        StartItemIndex = startItemIndex;
        EndItemIndex = endItemIndex;
    }
}