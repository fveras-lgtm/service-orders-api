using MediatR;

namespace Application.ServiceOrders.Commands.CreateServiceOrder;

/// <summary>
/// Command to open a new service order. The order is created in the Pending state with no technician assigned.
/// </summary>
public record CreateServiceOrderCommand(
    string CustomerName,
    string? CustomerPhone,
    string? CustomerEmail,
    string EquipmentType,
    string? EquipmentBrand,
    string? EquipmentModel,
    string? EquipmentSerialNumber,
    string ProblemDescription) : IRequest<CreateServiceOrderResult>;
