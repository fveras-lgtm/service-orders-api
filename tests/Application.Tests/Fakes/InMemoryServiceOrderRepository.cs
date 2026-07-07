using Application.Abstractions.Persistence;
using Domain.Entities;

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
}
