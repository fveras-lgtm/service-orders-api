namespace Domain.Entities;

/// <summary>
/// A technician who can be assigned to service orders.
/// </summary>
public class Technician
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Email { get; private set; }

    // Parameterless ctor for EF Core.
    private Technician()
    {
    }

    public Technician(string name, string? email = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = Guid.NewGuid();
        Name = name.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }
}
