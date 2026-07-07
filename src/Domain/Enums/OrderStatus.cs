namespace Domain.Enums;

/// <summary>
/// Lifecycle states of a <see cref="Entities.ServiceOrder"/>.
/// A newly created order starts as <see cref="Pending"/>.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    InProgress = 1,
    Closed = 2
}
