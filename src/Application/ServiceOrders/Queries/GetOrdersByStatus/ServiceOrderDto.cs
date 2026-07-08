namespace Application.ServiceOrders.Queries.GetOrdersByStatus;

/// <summary>
/// Flat read model for a service order. Contains no Domain types.
/// </summary>
public record ServiceOrderDto(
    Guid Id,
    string CustomerName,
    string EquipmentType,
    string ProblemDescription,
    Guid? TechnicianId,
    string Status);
