using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Teniry.Cqrs.Extended.Queryables.Filter;
using Teniry.Cqrs.SampleApi.Domain;

namespace Teniry.Cqrs.SampleApi.Application.GetTodos;

/// <remark>
///     <see cref="QueryableFilter{TEntity}"/> is a feature of Teniry.Cqrs.Extended package
/// </remark>
public class TodosFilter : QueryableFilter<Todo> {
    public string? Description { get; set; }

    /// <inheritdoc />
    public override Dictionary<string, Expression<Func<Todo, object>>> DefineSort() {
        return new() {
            { "description", x => x.Description },
            { "completed", x => x.Completed }
        };
    }

    /// <inheritdoc />
    protected override IQueryable<Todo> DefaultSort(IQueryable<Todo> query) {
        return query.OrderBy(x => x.Description);
    }

    /// <inheritdoc />
    protected override IQueryable<Todo> Filter(IQueryable<Todo> query) {
        if (Description != null) {
            query = query.Where(x => EF.Functions.ILike(x.Description, $"%{Description}%"));
        }

        return query;
    }
}