using FluentValidation;

namespace Teniry.Cqrs.SampleApi.Application.CreateTodo;

public class CreateTodoCommandValidator : AbstractValidator<CreateTodoCommand> {
    public CreateTodoCommandValidator() {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(100);
    }
}