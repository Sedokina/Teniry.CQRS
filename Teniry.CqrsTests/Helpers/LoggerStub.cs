using Microsoft.Extensions.Logging;

namespace Teniry.CqrsTests.Helpers;

public class LoggerStub<T> : ILogger<T> {
    private CallValidator _callValidator = new();
    public  List<string>  Calls => _callValidator.Calls;
    
    public void Log<TState>(
        LogLevel                         logLevel,
        EventId                          eventId,
        TState                           state,
        Exception?                       exception,
        Func<TState, Exception?, string> formatter
    ) {
        _callValidator.Called($"{logLevel} {exception?.GetType().Name} logged");
    }

    public bool IsEnabled(
        LogLevel logLevel
    ) {
        return true;
    }

    public IDisposable? BeginScope<TState>(
        TState state
    ) where TState : notnull {
        return null;
    }
}