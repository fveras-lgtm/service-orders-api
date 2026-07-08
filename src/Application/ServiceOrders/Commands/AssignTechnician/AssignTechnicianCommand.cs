using MediatR;

namespace Application.ServiceOrders.Commands.AssignTechnician;

/// <summary>
/// Command to assign a technician to a service order. Moves the order into the InProgress state.
/// </summary>
public record AssignTechnicianCommand(
    Guid ServiceOrderId,
    Guid TechnicianId) : IRequest<AssignTechnicianResult>;
