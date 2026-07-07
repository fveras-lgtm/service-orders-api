namespace Domain.ValueObjects;

/// <summary>
/// The equipment a service order is opened for. Immutable value object.
/// </summary>
public sealed record Equipment
{
    public string Type { get; }
    public string? Brand { get; }
    public string? Model { get; }
    public string? SerialNumber { get; }

    // Parameterless ctor for EF Core materialization of the owned type.
    private Equipment()
    {
        Type = null!;
    }

    public Equipment(string type, string? brand = null, string? model = null, string? serialNumber = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        Type = type.Trim();
        Brand = string.IsNullOrWhiteSpace(brand) ? null : brand.Trim();
        Model = string.IsNullOrWhiteSpace(model) ? null : model.Trim();
        SerialNumber = string.IsNullOrWhiteSpace(serialNumber) ? null : serialNumber.Trim();
    }
}
