using Application.Abstractions.Persistence;
using Domain.Entities;
using Domain.Enums;

namespace Application.Tests.Fakes;

/// <summary>
/// Hand-written in-memory fake for <see cref="IServiceOrderRepository"/>.
/// Exposes the stored orders so tests can assert on what was persisted.
/// </summary>
public class InMemoryServiceOrderRepository : IServiceOrderRepository
{
    public List<ServiceOrder> Orders { get; } = new();

    public Task AddAsync(ServiceOrder order, CancellationToken cancellationToken = default)
    {
        Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Orders.SingleOrDefault(order => order.Id == id));
    }

    public Task UpdateAsync(ServiceOrder order, CancellationToken cancellationToken = default)
    {
        // Orders are stored by reference; mutations are already reflected.
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ServiceOrder>> ListByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ServiceOrder> matches = Orders.Where(order => order.Status == status).ToList();
        return Task.FromResult(matches);
    }
}
