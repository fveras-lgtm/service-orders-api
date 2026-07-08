using Application.Abstractions.Persistence;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly AppDbContext _dbContext;

    public ServiceOrderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ServiceOrder order, CancellationToken cancellationToken = default)
    {
        await _dbContext.ServiceOrders.AddAsync(order, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceOrders
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(ServiceOrder order, CancellationToken cancellationToken = default)
    {
        _dbContext.ServiceOrders.Update(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceOrder>> ListByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceOrders
            .AsNoTracking()
            .Where(order => order.Status == status)
            .ToListAsync(cancellationToken);
    }
}
