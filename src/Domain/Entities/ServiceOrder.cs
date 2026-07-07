using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// A technical service order. Created in the <see cref="OrderStatus.Pending"/> state.
/// </summary>
public class ServiceOrder
{
    public Guid Id { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public Equipment Equipment { get; private set; } = null!;
    public string ProblemDescription { get; private set; } = null!;
    public Guid? TechnicianId { get; private set; }
    public OrderStatus Status { get; private set; }

    // Parameterless ctor for EF Core.
    private ServiceOrder()
    {
    }

    public ServiceOrder(Customer customer, Equipment equipment, string problemDescription)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(equipment);
        ArgumentException.ThrowIfNullOrWhiteSpace(problemDescription);

        Id = Guid.NewGuid();
        Customer = customer;
        Equipment = equipment;
        ProblemDescription = problemDescription.Trim();
        Status = OrderStatus.Pending;
    }

    /// <summary>
    /// Assigns a technician and moves the order into <see cref="OrderStatus.InProgress"/>.
    /// </summary>
    public void AssignTechnician(Guid technicianId)
    {
        if (technicianId == Guid.Empty)
            throw new ArgumentException("Technician id must not be empty.", nameof(technicianId));

        if (Status == OrderStatus.Closed)
            throw new InvalidOperationException("Cannot assign a technician to a closed order.");

        TechnicianId = technicianId;
        Status = OrderStatus.InProgress;
    }

    /// <summary>
    /// Closes the order.
    /// </summary>
    public void Close()
    {
        if (Status == OrderStatus.Closed)
            return;

        Status = OrderStatus.Closed;
    }
}
