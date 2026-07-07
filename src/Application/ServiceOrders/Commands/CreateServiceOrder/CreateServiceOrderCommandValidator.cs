using FluentValidation;

namespace Application.ServiceOrders.Commands.CreateServiceOrder;

public class CreateServiceOrderCommandValidator : AbstractValidator<CreateServiceOrderCommand>
{
    public CreateServiceOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.EquipmentType)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ProblemDescription)
            .NotEmpty()
            .MaximumLength(2000);

        // Optional, but must be a valid address when supplied.
        RuleFor(x => x.CustomerEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerEmail));
    }
}
