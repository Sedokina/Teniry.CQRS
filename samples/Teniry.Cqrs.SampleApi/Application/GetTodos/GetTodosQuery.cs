using Teniry.Cqrs.Extended.Queryables.Sort;

namespace Teniry.Cqrs.SampleApi.Application.GetTodos;

// IDefineSortable is a feature of Teniry.Cqrs.Extended package
public class GetTodosQuery : IDefineSortable {
    public string? Description { get; set; }

    /// <inheritdoc />
    public string[]? Sort { get; set; }

    /// <inheritdoc />
    public string[] GetSortKeys() {
        return ["description", "completed"];
    }
}