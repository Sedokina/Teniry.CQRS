using Microsoft.Extensions.Localization;

namespace Teniry.Cqrs.Extended.Exceptions;

public class EntityNotFoundException : ExceptionBase {
    public Type NotFoundType { get; set; }

    public EntityNotFoundException(Type type, string? message)
        : base(message) {
        NotFoundType = type;
    }

    public EntityNotFoundException(Type type)
        : base("Entity {0} not found") {
        NotFoundType = type;
    }

    /// <inheritdoc />
    protected override object[] GetFormatParams(IStringLocalizer stringLocalizer) {
        return [NotFoundType.Name];
    }
}