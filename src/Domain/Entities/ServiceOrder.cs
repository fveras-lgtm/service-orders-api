using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// A technical service order. Created in the <see cref="OrderStatus.Pending"/> state.
/// </summary>
public class ServiceOrder
{
    /// <summary>
    /// Unique identifier of the order, assigned when the order is created.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Contact details of the customer who requested the service.
    /// </summary>
    public Customer Customer { get; private set; } = null!;

    /// <summary>
    /// The equipment the order was opened for.
    /// </summary>
    public Equipment Equipment { get; private set; } = null!;

    /// <summary>
    /// Free-text description of the reported problem.
    /// </summary>
    public string ProblemDescription { get; private set; } = null!;

    /// <summary>
    /// Identifier of the assigned technician, or <c>null</c> while the order is unassigned.
    /// </summary>
    public Guid? TechnicianId { get; private set; }

    /// <summary>
    /// Current lifecycle state of the order.
    /// </summary>
    public OrderStatus Status { get; private set; }

    // Parameterless ctor for EF Core.
    private ServiceOrder()
    {
    }

    /// <summary>
    /// Creates a new service order in the <see cref="OrderStatus.Pending"/> state with no
    /// technician assigned.
    /// </summary>
    /// <param name="customer">Contact details of the requesting customer. Required.</param>
    /// <param name="equipment">The equipment the order is opened for. Required.</param>
    /// <param name="problemDescription">Description of the reported problem. Must not be null or whitespace.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="customer"/> or <paramref name="equipment"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="problemDescription"/> is null, empty, or whitespace.
    /// </exception>
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
    /// <param name="technicianId">Identifier of the technician to assign. Must not be <see cref="Guid.Empty"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="technicianId"/> is <see cref="Guid.Empty"/>.</exception>
    /// <exception cref="InvalidOperationException">The order is already <see cref="OrderStatus.Closed"/>.</exception>
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
    /// Closes the order, moving it into the <see cref="OrderStatus.Closed"/> state.
    /// Calling this on an already-closed order is a no-op.
    /// </summary>
    public void Close()
    {
        if (Status == OrderStatus.Closed)
            return;

        Status = OrderStatus.Closed;
    }
}
