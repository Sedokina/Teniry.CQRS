using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.Commands;
using Teniry.Cqrs.OperationRetries;
using Teniry.CqrsTests.Helpers;

namespace Teniry.CqrsTests.CoreTests.CommandTests.HasReturnValue;

public class RunWithoutTransactionWithRetryCommandWithReturnValueTests
{
    private readonly ServiceCollection _services;
    private readonly CallValidator _callValidator;

    public RunWithoutTransactionWithRetryCommandWithReturnValueTests()
    {
        _services = new ServiceCollection();
        _callValidator = new CallValidator();
        _services.AddScoped<CallValidator>(_ => _callValidator);
    }

    [Fact]
    public async Task Should_RetryHandle_When_DbExceptionThrown()
    {
        // Arrange
        _services.AddScoped<ICommandHandler<CustomRetryCommand, string>, CustomRetryHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider());

        // Act
        var exception = new TestRetryException();
        var result = await dispatcher
            .DispatchAsync<CustomRetryCommand, string>(new CustomRetryCommand(2, exception), new());

        // Assert
        result.Should().Be("Handler called");
        _callValidator.Calls.Should()
            .SatisfyRespectively(
                first => first.Should().Be("Handler called with error 1"),
                second => second.Should().Be("Handler called with error 2"),
                third => third.Should().Be("Handler called successfully 3"));
    }

    [Fact]
    public async Task Should_ThrowException_When_RetryAttemptsExceeded()
    {
        // Arrange
        _services.AddScoped<ICommandHandler<CustomRetryCommand, string>, CustomRetryHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider());

        // Act
        var exception = new TestRetryException();
        var act = async () => await dispatcher
            .DispatchAsync<CustomRetryCommand, string>(new CustomRetryCommand(5, exception), new());

        // Assert
        await act.Should().ThrowAsync<TestRetryException>();
        _callValidator.Calls.Should()
            .SatisfyRespectively(
                first => first.Should().Be("Handler called with error 1"),
                second => second.Should().Be("Handler called with error 2"),
                third => third.Should().Be("Handler called with error 3"));
    }

    [Fact]
    public async Task Should_NotRetryOnAnyException_Except_CustomExceptionSetInHandler()
    {
        // Arrange
        _services.AddScoped<ICommandHandler<CustomRetryCommand, string>, CustomRetryHandler>();
        var dispatcher = new CommandDispatcher(_services.BuildServiceProvider());

        // Act
        var exception = new Exception();
        var act = async () =>
            await dispatcher.DispatchAsync<CustomRetryCommand, string>(new CustomRetryCommand(5, exception), new());

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _callValidator.Calls.Should()
            .SatisfyRespectively(first => first.Should().Be("Handler called with error 1"));
    }

    private class CustomRetryCommand(int timesToFail, Exception throwOnFail)
    {
        public int TimesToFail { get; set; } = timesToFail;
        public Exception ThrowOnFail { get; } = throwOnFail;
    }

    private class CustomRetryHandler : ICommandHandler<CustomRetryCommand, string>, IRetriableOperation
    {
        private readonly CallValidator _callValidator;
        private int _timesFailed = 0;

        public CustomRetryHandler(CallValidator callValidator)
        {
            _callValidator = callValidator;
        }

        public Task<string> HandleAsync(
            CustomRetryCommand command,
            CancellationToken cancellation
        )
        {
            if (_timesFailed == command.TimesToFail)
            {
                _callValidator.Called($"Handler called successfully {_timesFailed + 1}");
                return Task.FromResult("Handler called");
            }

            _callValidator.Called($"Handler called with error {_timesFailed + 1}");
            _timesFailed++;
            throw command.ThrowOnFail;
        }

        public int GetMaxRetryAttempts()
        {
            return 3;
        }

        public bool RetryOnException(Exception ex)
        {
            return ex is TestRetryException;
        }
    }

    private class TestRetryException : Exception
    {
    }
}