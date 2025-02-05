using Microsoft.AspNetCore.Mvc;
using Teniry.Cqrs.Commands;
using Teniry.Cqrs.Queries;
using Teniry.Cqrs.SampleApi.Application.CompleteTodo;
using Teniry.Cqrs.SampleApi.Application.CreateTodo;
using Teniry.Cqrs.SampleApi.Application.GetTodos;

namespace Teniry.Cqrs.SampleApi.Endpoints;

public static class Todos {
    /// <summary>
    ///     Get all todos list
    /// </summary>
    /// <response code="200">Returns todos list</response>
    [ProducesResponseType(typeof(List<TodoDto>), 200)]
    public static async Task<IResult> GetTodosAsync(
        [AsParameters] GetTodosQuery query,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken
    ) {
        var result = await queryDispatcher.DispatchAsync<GetTodosQuery, List<TodoDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }

    /// <summary>
    ///     Create new todo
    /// </summary>
    /// <response code="201">New todo created</response>
    [ProducesResponseType(201)]
    public static async Task<IResult> CreateTodoAsync(
        CreateTodoCommand command,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken
    ) {
        var result = await commandDispatcher
            .DispatchAsync<CreateTodoCommand, CreatedTodoDto>(command, cancellationToken);

        return TypedResults.Created();
    }

    /// <summary>
    ///     Complete todo
    /// </summary>
    /// <response code="204">Todo completed and closed</response>
    [ProducesResponseType(201)]
    public static async Task<IResult> CompleteTodoAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken
    ) {
        await commandDispatcher.DispatchAsync(new CompleteTodoCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}