namespace Teniry.Cqrs.OperationRetries;

public interface IRetriableOperation {
    const int DefaultRetryAttempts = 5;

    /// <summary>
    ///     Get max attempts to run handler on any failure
    /// </summary>
    /// <returns>Max attempts</returns>
    int GetMaxRetryAttempts() {
        return DefaultRetryAttempts;
    }

    bool RetryOnException(Exception ex) {
        return true;
    }

    /// <summary>
    ///     If handler made changes to any object or state while executing
    ///     but on retry this object or state should be in the initial state
    ///     all cleanup methods should be declared in this function,
    ///     and it would be called to clean up all changes after failure run
    /// </summary>
    ValueTask CleanupBeforeRetryAsync() {
        return ValueTask.CompletedTask;
    }
}