namespace Domain.ValueObjects;

/// <summary>
/// Customer contact details captured on a service order. Immutable value object.
/// </summary>
public sealed record Customer
{
    public string Name { get; }
    public string? Phone { get; }
    public string? Email { get; }

    // Parameterless ctor for EF Core materialization of the owned type.
    private Customer()
    {
        Name = null!;
    }

    public Customer(string name, string? phone = null, string? email = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }
}
