using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Teniry.Cqrs.Queries;

namespace Teniry.CqrsTests.CoreTests;

public class QueriesTests {
    private readonly ServiceCollection _services;

    public QueriesTests() {
        _services = new();
    }

    [Fact]
    public async Task Should_RunHandler_On_QueryDispatch() {
        // Arrange
        _services.AddScoped<IQueryHandler<GetDataQuery, DataResult>, GetDataHandler>();
        var dispatcher = new QueryDispatcher(_services.BuildServiceProvider());

        // Act
        var handlerResult = await dispatcher.DispatchAsync<GetDataQuery, DataResult>(new(1), new());

        // Assert
        handlerResult.Should().NotBeNull();
        handlerResult.Result.Should().Be("handled 1");
    }

    [Fact]
    public async Task Should_ThrowException_When_QueryNotRegisteredInServices() {
        // Arrange
        var dispatcher = new QueryDispatcher(_services.BuildServiceProvider());

        // Act
        var act = async () => await dispatcher.DispatchAsync<GetDataQuery, DataResult>(new(1), new());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private class GetDataQuery(int id) {
        public int Id { get; } = id;
    }

    private class GetDataHandler : IQueryHandler<GetDataQuery, DataResult> {
        public Task<DataResult> HandleAsync(
            GetDataQuery query,
            CancellationToken cancellation
        ) {
            return Task.FromResult(new DataResult($"handled {query.Id}"));
        }
    }

    private class DataResult(string result) {
        public string Result { get; } = result;
    }
}