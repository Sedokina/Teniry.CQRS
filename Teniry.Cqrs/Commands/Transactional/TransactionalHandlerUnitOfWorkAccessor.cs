using Microsoft.Extensions.DependencyInjection;

namespace Teniry.Cqrs.Commands.Transactional;

internal static class TransactionalHandlerUnitOfWorkAccessor {
    /// <summary>
    ///     Get unit of work of type set in the <see cref="ITransactionalHandler{T}"/> interface of the handler
    /// </summary>
    /// <param name="handler">Object of the class with <see cref="ITransactionalHandler{T}"/> interface</param>
    /// <param name="serviceProvider">Service provider to use to get IUnitOfWork object</param>
    /// <returns>Unit of work object of type from <see cref="ITransactionalHandler{T}"/></returns>
    /// <exception cref="InvalidOperationException">Handler has no <see cref="ITransactionalHandler{T}"/> interface</exception>
    internal static IUnitOfWork GetUnitOfWork(
        object           handler,
        IServiceProvider serviceProvider
    ) {
        var unitOfWork = handler
            .GetType()
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITransactionalHandler<>))
            .SelectMany(i => i.GetGenericArguments())
            .FirstOrDefault();

        if (unitOfWork == null) {
            throw new InvalidOperationException($"No {nameof(ITransactionalHandler)}<> found for the {handler.GetType().Name}");
        }

        return (IUnitOfWork)serviceProvider.GetRequiredService(unitOfWork);
    }
}