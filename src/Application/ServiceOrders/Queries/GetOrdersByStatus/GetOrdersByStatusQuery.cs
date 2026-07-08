using Domain.Enums;
using MediatR;

namespace Application.ServiceOrders.Queries.GetOrdersByStatus;

/// <summary>
/// Query to list service orders filtered by their <see cref="OrderStatus"/>.
/// </summary>
public record GetOrdersByStatusQuery(OrderStatus Status)
    : IRequest<IReadOnlyList<ServiceOrderDto>>;
