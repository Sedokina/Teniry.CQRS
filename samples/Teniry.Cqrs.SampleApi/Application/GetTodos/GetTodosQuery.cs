using Teniry.Cqrs.Extended.Queryables.Page;
using Teniry.Cqrs.Extended.Queryables.Sort;

namespace Teniry.Cqrs.SampleApi.Application.GetTodos;

/// <summary>
///     <see cref="IPage"/> and <see cref="IDefineSortable"/> is a feature of Teniry.Cqrs.Extended package
/// </summary>
public class GetTodosQuery : IPage, IDefineSortable {
    public string? Description { get; set; }

    /// <inheritdoc />
    public int Page { get; set; }

    /// <inheritdoc />
    public int PageSize { get; set; }

    /// <inheritdoc />
    public string[]? Sort { get; set; }

    /// <inheritdoc />
    public string[] GetSortKeys() {
        return ["description", "completed"];
    }
}