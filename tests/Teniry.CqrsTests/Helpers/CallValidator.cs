namespace Teniry.CqrsTests.Helpers;

internal class CallValidator {
    public List<string> Calls { get; } = [];

    public virtual void Called(
        string message
    ) {
        Calls.Add(message);
    }
}