using Application.ServiceOrders.Commands.CreateServiceOrder;

namespace Application.Tests.ServiceOrders.Commands.CreateServiceOrder;

public class CreateServiceOrderCommandValidatorTests
{
    private readonly CreateServiceOrderCommandValidator _validator = new();

    private static CreateServiceOrderCommand ValidCommand() => new(
        CustomerName: "Ada Lovelace",
        CustomerPhone: null,
        CustomerEmail: null,
        EquipmentType: "Laptop",
        EquipmentBrand: null,
        EquipmentModel: null,
        EquipmentSerialNumber: null,
        ProblemDescription: "Does not power on.");

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_customer_name_fails(string name)
    {
        var result = _validator.Validate(ValidCommand() with { CustomerName = name });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateServiceOrderCommand.CustomerName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_equipment_type_fails(string type)
    {
        var result = _validator.Validate(ValidCommand() with { EquipmentType = type });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateServiceOrderCommand.EquipmentType));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_problem_description_fails(string description)
    {
        var result = _validator.Validate(ValidCommand() with { ProblemDescription = description });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateServiceOrderCommand.ProblemDescription));
    }

    [Fact]
    public void Malformed_email_fails()
    {
        var result = _validator.Validate(ValidCommand() with { CustomerEmail = "not-an-email" });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateServiceOrderCommand.CustomerEmail));
    }

    [Fact]
    public void Null_email_passes()
    {
        var result = _validator.Validate(ValidCommand() with { CustomerEmail = null });

        Assert.True(result.IsValid);
    }
}
