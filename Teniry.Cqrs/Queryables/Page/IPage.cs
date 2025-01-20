namespace Teniry.Cqrs.Queryables.Page;

public interface IPage {
    int Page     { get; set; }
    int PageSize { get; set; }
}