using Application.ServiceOrders.Queries.GetOrdersByStatus;
using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Tests.ServiceOrders.Queries.GetOrdersByStatus;

public class GetOrdersByStatusQueryHandlerTests
{
    private static ServiceOrder NewOrder(string customerName = "Ada Lovelace") => new(
        new Customer(customerName, null, null),
        new Equipment("Laptop", null, null, null),
        "Does not power on.");

    [Fact]
    public async Task Handle_returns_only_orders_in_the_requested_status()
    {
        var pending = NewOrder("Pending Pat");

        var inProgress = NewOrder("Busy Bob");
        inProgress.AssignTechnician(Guid.NewGuid());

        var repository = new InMemoryServiceOrderRepository();
        repository.Orders.Add(pending);
        repository.Orders.Add(inProgress);
        var handler = new GetOrdersByStatusQueryHandler(repository);

        var result = await handler.Handle(
            new GetOrdersByStatusQuery(OrderStatus.InProgress),
            CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal(inProgress.Id, dto.Id);
        Assert.Equal(nameof(OrderStatus.InProgress), dto.Status);
    }

    [Fact]
    public async Task Handle_maps_domain_fields_onto_the_dto()
    {
        var order = NewOrder();
        var technicianId = Guid.NewGuid();
        order.AssignTechnician(technicianId);

        var repository = new InMemoryServiceOrderRepository();
        repository.Orders.Add(order);
        var handler = new GetOrdersByStatusQueryHandler(repository);

        var result = await handler.Handle(
            new GetOrdersByStatusQuery(OrderStatus.InProgress),
            CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal(order.Id, dto.Id);
        Assert.Equal(order.Customer.Name, dto.CustomerName);
        Assert.Equal(order.Equipment.Type, dto.EquipmentType);
        Assert.Equal(order.ProblemDescription, dto.ProblemDescription);
        Assert.Equal(technicianId, dto.TechnicianId);
    }

    [Fact]
    public async Task Handle_returns_empty_when_no_orders_match()
    {
        var repository = new InMemoryServiceOrderRepository();
        repository.Orders.Add(NewOrder());
        var handler = new GetOrdersByStatusQueryHandler(repository);

        var result = await handler.Handle(
            new GetOrdersByStatusQuery(OrderStatus.Closed),
            CancellationToken.None);

        Assert.Empty(result);
    }
}
