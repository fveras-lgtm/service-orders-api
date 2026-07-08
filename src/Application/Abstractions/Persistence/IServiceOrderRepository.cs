using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions.Persistence;

/// <summary>
/// Persistence abstraction for <see cref="ServiceOrder"/> aggregates.
/// Implemented in the Infrastructure layer.
/// </summary>
public interface IServiceOrderRepository
{
    /// <summary>
    /// Adds a new service order and persists it.
    /// </summary>
    Task AddAsync(ServiceOrder order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a service order by its id, or <c>null</c> if none exists.
    /// </summary>
    Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an existing service order.
    /// </summary>
    Task UpdateAsync(ServiceOrder order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists service orders in the given <paramref name="status"/>.
    /// </summary>
    Task<IReadOnlyList<ServiceOrder>> ListByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default);
}
