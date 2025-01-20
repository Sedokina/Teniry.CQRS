namespace Teniry.Cqrs.Queryables.Page;

public class PagedResult<T> {
    public List<T>  Items { get; set; }
    public PageInfo Page  { get; set; }

    public PagedResult(List<T> items, PageInfo page) {
        Items = items;
        Page  = page;
    }
}