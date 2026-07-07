using Application.Abstractions.Persistence;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;

namespace Application.ServiceOrders.Commands.CreateServiceOrder;

public class CreateServiceOrderCommandHandler
    : IRequestHandler<CreateServiceOrderCommand, CreateServiceOrderResult>
{
    private readonly IServiceOrderRepository _repository;

    public CreateServiceOrderCommandHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateServiceOrderResult> Handle(
        CreateServiceOrderCommand request,
        CancellationToken cancellationToken)
    {
        var customer = new Customer(request.CustomerName, request.CustomerPhone, request.CustomerEmail);
        var equipment = new Equipment(
            request.EquipmentType,
            request.EquipmentBrand,
            request.EquipmentModel,
            request.EquipmentSerialNumber);

        var order = new ServiceOrder(customer, equipment, request.ProblemDescription);

        await _repository.AddAsync(order, cancellationToken);

        return new CreateServiceOrderResult(order.Id);
    }
}
