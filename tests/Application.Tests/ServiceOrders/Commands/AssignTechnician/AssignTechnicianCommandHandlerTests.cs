using Application.ServiceOrders.Commands.AssignTechnician;
using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Tests.ServiceOrders.Commands.AssignTechnician;

public class AssignTechnicianCommandHandlerTests
{
    private static ServiceOrder PendingOrder() => new(
        new Customer("Ada Lovelace", null, null),
        new Equipment("Laptop", null, null, null),
        "Does not power on.");

    [Fact]
    public async Task Handle_assigns_technician_and_moves_order_to_InProgress()
    {
        var order = PendingOrder();
        var repository = new InMemoryServiceOrderRepository();
        repository.Orders.Add(order);
        var handler = new AssignTechnicianCommandHandler(repository);
        var technicianId = Guid.NewGuid();

        var result = await handler.Handle(
            new AssignTechnicianCommand(order.Id, technicianId),
            CancellationToken.None);

        Assert.Equal(order.Id, result.Id);
        Assert.Equal(technicianId, result.TechnicianId);
        Assert.Equal(nameof(OrderStatus.InProgress), result.Status);

        Assert.Equal(technicianId, order.TechnicianId);
        Assert.Equal(OrderStatus.InProgress, order.Status);
    }

    [Fact]
    public async Task Handle_throws_when_order_does_not_exist()
    {
        var repository = new InMemoryServiceOrderRepository();
        var handler = new AssignTechnicianCommandHandler(repository);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(
                new AssignTechnicianCommand(Guid.NewGuid(), Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_throws_when_order_is_closed()
    {
        var order = PendingOrder();
        order.Close();
        var repository = new InMemoryServiceOrderRepository();
        repository.Orders.Add(order);
        var handler = new AssignTechnicianCommandHandler(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new AssignTechnicianCommand(order.Id, Guid.NewGuid()),
                CancellationToken.None));
    }
}
