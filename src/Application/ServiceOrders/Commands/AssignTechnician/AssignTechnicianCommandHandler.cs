using Application.Abstractions.Persistence;
using MediatR;

namespace Application.ServiceOrders.Commands.AssignTechnician;

public class AssignTechnicianCommandHandler
    : IRequestHandler<AssignTechnicianCommand, AssignTechnicianResult>
{
    private readonly IServiceOrderRepository _repository;

    public AssignTechnicianCommandHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<AssignTechnicianResult> Handle(
        AssignTechnicianCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.ServiceOrderId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Service order '{request.ServiceOrderId}' was not found.");

        order.AssignTechnician(request.TechnicianId);

        await _repository.UpdateAsync(order, cancellationToken);

        return new AssignTechnicianResult(order.Id, order.TechnicianId!.Value, order.Status.ToString());
    }
}
