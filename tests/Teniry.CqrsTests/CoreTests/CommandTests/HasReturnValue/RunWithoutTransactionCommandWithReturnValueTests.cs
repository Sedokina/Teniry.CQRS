using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.Commands;

namespace Teniry.CqrsTests.CoreTests.CommandTests.HasReturnValue;

public class RunWithoutTransactionCommandWithReturnValueTests {
    private readonly ServiceCollection _serviceCollection;

    public RunWithoutTransactionCommandWithReturnValueTests() {
        _serviceCollection = new();
    }

    [Fact]
    public async Task Should_RunHandler_When_NoValidatorRegistered() {
        // Arrange
        _serviceCollection.AddScoped<ICommandHandler<UpdateTestDataCommand, string>, UpdateTestDataHandler>();
        var dispatcher = new CommandDispatcher(_serviceCollection.BuildServiceProvider());

        // Act
        var result = await dispatcher
            .DispatchAsync<UpdateTestDataCommand, string>(new(Guid.Empty, string.Empty), new());

        // Assert
        result.Should().Be("Handler called");
    }

    [Fact]
    public async Task Should_ThrowException_When_CommandIsNotValid() {
        // Arrange
        _serviceCollection.AddScoped<ICommandHandler<UpdateTestDataCommand, string>, UpdateTestDataHandler>();
        _serviceCollection.AddScoped<IValidator<UpdateTestDataCommand>, UpdateTestDataValidator>();
        var dispatcher = new CommandDispatcher(_serviceCollection.BuildServiceProvider());

        // Act
        var act = async () => await dispatcher
            .DispatchAsync<UpdateTestDataCommand, string>(new(Guid.Empty, string.Empty), new());

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    private class UpdateTestDataCommand(Guid id, string text) {
        public Guid Id { get; } = id;
        public string Text { get; } = text;
    }

    private class UpdateTestDataValidator : AbstractValidator<UpdateTestDataCommand> {
        public UpdateTestDataValidator() {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Text).NotEmpty();
        }
    }

    private class UpdateTestDataHandler : ICommandHandler<UpdateTestDataCommand, string> {
        public Task<string> HandleAsync(
            UpdateTestDataCommand command,
            CancellationToken cancellation
        ) {
            return Task.FromResult("Handler called");
        }
    }
}