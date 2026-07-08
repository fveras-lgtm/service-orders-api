using Application.Abstractions.Persistence;
using MediatR;

namespace Application.ServiceOrders.Queries.GetOrdersByStatus;

public class GetOrdersByStatusQueryHandler
    : IRequestHandler<GetOrdersByStatusQuery, IReadOnlyList<ServiceOrderDto>>
{
    private readonly IServiceOrderRepository _repository;

    public GetOrdersByStatusQueryHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ServiceOrderDto>> Handle(
        GetOrdersByStatusQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _repository.ListByStatusAsync(request.Status, cancellationToken);

        return orders
            .Select(order => new ServiceOrderDto(
                order.Id,
                order.Customer.Name,
                order.Equipment.Type,
                order.ProblemDescription,
                order.TechnicianId,
                order.Status.ToString()))
            .ToList();
    }
}
