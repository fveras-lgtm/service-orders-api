using Domain.Entities;

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
}
