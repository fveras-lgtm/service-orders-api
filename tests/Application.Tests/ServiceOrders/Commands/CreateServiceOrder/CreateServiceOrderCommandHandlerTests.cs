using Application.ServiceOrders.Commands.CreateServiceOrder;
using Application.Tests.Fakes;
using Domain.Enums;

namespace Application.Tests.ServiceOrders.Commands.CreateServiceOrder;

public class CreateServiceOrderCommandHandlerTests
{
    private static CreateServiceOrderCommand ValidCommand() => new(
        CustomerName: "Ada Lovelace",
        CustomerPhone: "809-555-0101",
        CustomerEmail: "ada@example.com",
        EquipmentType: "Laptop",
        EquipmentBrand: "Dell",
        EquipmentModel: "XPS 13",
        EquipmentSerialNumber: "SN-12345",
        ProblemDescription: "Does not power on.");

    [Fact]
    public async Task Handle_persists_one_order_and_returns_its_id()
    {
        var repository = new InMemoryServiceOrderRepository();
        var handler = new CreateServiceOrderCommandHandler(repository);

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        var order = Assert.Single(repository.Orders);
        Assert.Equal(order.Id, result.Id);
    }

    [Fact]
    public async Task Handle_creates_order_in_Pending_state_with_no_technician()
    {
        var repository = new InMemoryServiceOrderRepository();
        var handler = new CreateServiceOrderCommandHandler(repository);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        var order = Assert.Single(repository.Orders);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Null(order.TechnicianId);
    }

    [Fact]
    public async Task Handle_maps_command_fields_onto_the_created_order()
    {
        var repository = new InMemoryServiceOrderRepository();
        var handler = new CreateServiceOrderCommandHandler(repository);
        var command = ValidCommand();

        await handler.Handle(command, CancellationToken.None);

        var order = Assert.Single(repository.Orders);
        Assert.Equal(command.CustomerName, order.Customer.Name);
        Assert.Equal(command.CustomerPhone, order.Customer.Phone);
        Assert.Equal(command.CustomerEmail, order.Customer.Email);
        Assert.Equal(command.EquipmentType, order.Equipment.Type);
        Assert.Equal(command.EquipmentBrand, order.Equipment.Brand);
        Assert.Equal(command.EquipmentModel, order.Equipment.Model);
        Assert.Equal(command.EquipmentSerialNumber, order.Equipment.SerialNumber);
        Assert.Equal(command.ProblemDescription, order.ProblemDescription);
    }
}
