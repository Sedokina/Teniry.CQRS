using Teniry.Cqrs.Types.PatchOperationType.Exceptions;

namespace Teniry.Cqrs.Types.PatchOperationType;

public class PatchOp<T> {
    public T? Value { get; set; }
    public PatchOpType Type { get; set; }

    public PatchOp(T? value, PatchOpType type) {
        Value = value;
        Type = type;
    }
}

public static class PatchOp {
    public static async Task<bool> HandleAsync<T>(
        PatchOp<T>? op,
        string property,
        Func<T, CancellationToken, Task>? updateAsync = null,
        Func<CancellationToken, Task>? removeAsync = null,
        CancellationToken cancellation = default
    ) {
        if (op is null) return false;

        switch (op.Type) {
            case PatchOpType.Update when updateAsync is not null:
                if (op.Value is null) {
                    throw new PatchOperationNotAllowedWithoutValueException(op.Type, property);
                }

                await updateAsync(op.Value, cancellation);

                return true;
            case PatchOpType.Remove when removeAsync is not null:
                await removeAsync(cancellation);

                return true;
            default:
                throw new PatchOperationNotAllowedException(op.Type, property);
        }
    }

    public static bool Handle<T>(
        PatchOp<T>? op,
        string property,
        Action<T>? update = null,
        Action? remove = null
    ) {
        if (op is null) return false;

        switch (op.Type) {
            case PatchOpType.Update when update is not null:
                if (op.Value is null) {
                    throw new PatchOperationNotAllowedWithoutValueException(op.Type, property);
                }

                update(op.Value);

                return true;
            case PatchOpType.Remove when remove is not null:
                remove();

                return true;
            default:
                throw new PatchOperationNotAllowedException(op.Type, property);
        }
    }

    public static PatchOp<T> Update<T>(T value) {
        return new(value, PatchOpType.Update);
    }

    public static PatchOp<T> Remove<T>(T value) {
        return new(value, PatchOpType.Remove);
    }
}