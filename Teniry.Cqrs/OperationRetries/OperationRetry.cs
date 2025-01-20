namespace Teniry.Cqrs.OperationRetries;

public static class OperationRetry {
    /// <summary>
    ///     Run <b>actionToRetry</b> several times if it throws <see cref="InvalidOperationException" />
    /// </summary>
    /// <param name="actionToRetry">Action to be repeated</param>
    /// <param name="retriableOperation">Max number of attempts to run the action if action fails and cleanup method</param>
    /// <exception cref="Exception">When no attempts left, throws exception occured at the last attempt</exception>
    public static async Task RetryOnFailAsync(
        Func<Task> actionToRetry,
        IRetriableOperation retriableOperation
    ) {
        var maxAttempts = retriableOperation.GetMaxRetryAttempts();
        var attempt = 0;

        do {
            attempt++;

            try {
                await actionToRetry().ConfigureAwait(false);

                return;
            } catch (Exception ex) {
                if (!retriableOperation.RetryOnException(ex)) {
                    throw;
                }

                if (attempt >= maxAttempts) {
                    throw;
                }

                await retriableOperation.CleanupBeforeRetryAsync().ConfigureAwait(false);
            }
        } while (attempt <= maxAttempts);
    }

    /// <summary>
    ///     Run <b>actionToRetry</b> several times if it throws <see cref="InvalidOperationException" />
    /// </summary>
    /// <param name="actionToRetry">Action to be repeated</param>
    /// <param name="retriableOperation">Max number of attempts to run the action if action fails and cleanup method</param>
    /// <exception cref="Exception">When no attempts left, throws exception occured at the last attempt</exception>
    public static async Task<T> RetryOnFailAsync<T>(
        Func<Task<T>> actionToRetry,
        IRetriableOperation retriableOperation
    ) {
        var maxAttempts = retriableOperation.GetMaxRetryAttempts();
        var attempt = 0;

        do {
            attempt++;

            try {
                return await actionToRetry().ConfigureAwait(false);
            } catch (Exception ex) {
                if (!retriableOperation.RetryOnException(ex)) {
                    throw;
                }

                if (attempt >= maxAttempts) {
                    throw;
                }

                await retriableOperation.CleanupBeforeRetryAsync().ConfigureAwait(false);
            }
        } while (attempt <= maxAttempts);

        // This is an unreachable exception
        throw new MaxRetryAttemptsReachedException(maxAttempts);
    }
}