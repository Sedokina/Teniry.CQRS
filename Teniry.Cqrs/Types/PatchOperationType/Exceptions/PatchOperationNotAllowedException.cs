using Microsoft.Extensions.Localization;
using Teniry.Cqrs.Exceptions;

namespace Teniry.Cqrs.Types.PatchOperationType.Exceptions;

public class PatchOperationNotAllowedException : ExceptionBase {
    public PatchOpType OpType { get; }
    public string Property { get; }

    public PatchOperationNotAllowedException(PatchOpType opType, string property)
        : base("Patch operation of type {0} not allowed for property {1}") {
        OpType = opType;
        Property = property;
    }

    /// <inheritdoc />
    protected override object[] GetFormatParams(IStringLocalizer stringLocalizer) {
        return new object[] { OpType.ToString(), Property };
    }
}