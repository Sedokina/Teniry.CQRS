using Microsoft.Extensions.Localization;
using Teniry.Cqrs.Extended.Exceptions;

namespace Teniry.Cqrs.Extended.Types.PatchOperationType.Exceptions;

public class PatchOperationNotAllowedWithoutValueException : ExceptionBase {
    public PatchOpType OpType { get; }
    public string Property { get; }

    public PatchOperationNotAllowedWithoutValueException(PatchOpType opType, string property)
        : base("Patch operation of type {0} not allowed for property {1}") {
        OpType = opType;
        Property = property;
    }

    /// <inheritdoc />
    protected override object[] GetFormatParams(IStringLocalizer stringLocalizer) {
        return new object[] { OpType.ToString(), Property };
    }
}