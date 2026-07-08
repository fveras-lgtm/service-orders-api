namespace Application.ServiceOrders.Commands.AssignTechnician;

/// <summary>
/// Result of <see cref="AssignTechnicianCommand"/>: the affected order id, the assigned
/// technician, and the resulting status.
/// </summary>
public record AssignTechnicianResult(Guid Id, Guid TechnicianId, string Status);
