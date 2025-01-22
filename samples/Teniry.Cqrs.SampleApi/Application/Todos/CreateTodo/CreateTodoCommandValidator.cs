using FluentValidation;

namespace Teniry.Cqrs.SampleApi.Application.Todos.CreateTodo;

public class CreateTodoCommandValidator : AbstractValidator<CreateTodoCommand> {
    public CreateTodoCommandValidator() {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(100);
    }
}