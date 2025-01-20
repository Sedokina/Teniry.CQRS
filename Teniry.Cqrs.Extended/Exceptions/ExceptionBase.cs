using Microsoft.Extensions.Localization;

namespace Teniry.Cqrs.Extended.Exceptions;

/// <summary>
///     This is the base class for all the exceptions in the application
///     which message can be shown to the user <br />
///     BEWARE: only message property can be shown, other properties stores sensitive data
/// </summary>
/// <remarks>
///     To get user friendly message, use <see cref="Format" /> method, it formats and translates exception message <br />
///     When application global exception handler catches <see cref="ExceptionBase" />
///     or any of it's inheritors, application global exception handler returns 4xx status codes <br /> <br />
///     You still can use <see cref="Exception" /> class instead of <see cref="ExceptionBase" /> <br />
///     if application global exception handler catches <see cref="Exception" /> or any of it's inheritors
///     (except <see cref="ExceptionBase" /> and it's inheritors), application global exception handler returns 5xx status
///     code
/// </remarks>
public class ExceptionBase : Exception {
    protected ExceptionBase(string? message)
        : base(message) { }

    protected ExceptionBase(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    ///     Format exception message to user friendly format
    /// </summary>
    /// <param name="stringLocalizer">Localizer which can translate message</param>
    /// <returns>Returns formatted and translated message</returns>
    public virtual string Format(IStringLocalizer stringLocalizer) {
        return stringLocalizer[Message, GetFormatParams(stringLocalizer)];
    }

    protected virtual object[] GetFormatParams(IStringLocalizer stringLocalizer) {
        return [];
    }
}