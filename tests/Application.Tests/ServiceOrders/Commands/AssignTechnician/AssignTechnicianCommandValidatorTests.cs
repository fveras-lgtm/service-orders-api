using Application.ServiceOrders.Commands.AssignTechnician;

namespace Application.Tests.ServiceOrders.Commands.AssignTechnician;

public class AssignTechnicianCommandValidatorTests
{
    private readonly AssignTechnicianCommandValidator _validator = new();

    private static AssignTechnicianCommand ValidCommand() => new(
        ServiceOrderId: Guid.NewGuid(),
        TechnicianId: Guid.NewGuid());

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Empty_service_order_id_fails()
    {
        var result = _validator.Validate(ValidCommand() with { ServiceOrderId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignTechnicianCommand.ServiceOrderId));
    }

    [Fact]
    public void Empty_technician_id_fails()
    {
        var result = _validator.Validate(ValidCommand() with { TechnicianId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AssignTechnicianCommand.TechnicianId));
    }
}
