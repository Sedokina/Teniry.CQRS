using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.Commands;

namespace Teniry.CqrsTests.CoreTests.CommandTests.NoReturnValue;

public class RunWithoutTransactionCommandTests {
    private readonly ServiceCollection _services;

    public RunWithoutTransactionCommandTests() {
        _services = new ServiceCollection();
    }

    [Fact]
    public async Task Should_RunHandler_When_NoValidatorRegistered() {
        // Arrange
        _services.AddScoped<ICommandHandler<UpdateTestDataCommand>, UpdateTestDataHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider());

        // Act
        var act = async () =>
            await dispatcher.DispatchAsync(new UpdateTestDataCommand(Guid.Empty, string.Empty), new());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Handler called");
    }

    [Fact]
    public async Task Should_ThrowException_When_CommandIsNotValid() {
        // Arrange
        _services.AddScoped<ICommandHandler<UpdateTestDataCommand>, UpdateTestDataHandler>();
        _services.AddScoped<IValidator<UpdateTestDataCommand>, UpdateTestDataValidator>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider());

        // Act
        var act = async () =>
            await dispatcher.DispatchAsync(new UpdateTestDataCommand(Guid.Empty, string.Empty), new());

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    private class UpdateTestDataCommand(Guid id, string text) {
        public Guid   Id   { get; set; } = id;
        public string Text { get; set; } = text;
    }

    private class UpdateTestDataValidator : AbstractValidator<UpdateTestDataCommand> {
        public UpdateTestDataValidator() {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Text).NotEmpty();
        }
    }

    private class UpdateTestDataHandler : ICommandHandler<UpdateTestDataCommand> {
        public Task HandleAsync(
            UpdateTestDataCommand command,
            CancellationToken     cancellation
        ) {
            throw new InvalidOperationException("Handler called");
        }
    }
}