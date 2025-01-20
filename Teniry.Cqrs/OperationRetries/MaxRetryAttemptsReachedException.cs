namespace Teniry.Cqrs.OperationRetries;

public class MaxRetryAttemptsReachedException(int retryAttempts)
    : Exception($"Max retry attempts {retryAttempts} reached");