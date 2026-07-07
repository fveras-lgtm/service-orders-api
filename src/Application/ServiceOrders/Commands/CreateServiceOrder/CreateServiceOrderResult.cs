namespace Application.ServiceOrders.Commands.CreateServiceOrder;

/// <summary>
/// Result of <see cref="CreateServiceOrderCommand"/>: the id of the newly created order.
/// </summary>
public record CreateServiceOrderResult(Guid Id);
