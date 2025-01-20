using Teniry.Cqrs.Exceptions;

namespace Teniry.Cqrs.OperationRetries;

public class MaxRetryAttemptsLimitException : ExceptionBase {
    public MaxRetryAttemptsLimitException()
        : base("Max retry attempts limit") { }
}