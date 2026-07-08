using FluentValidation;

namespace Application.ServiceOrders.Commands.AssignTechnician;

public class AssignTechnicianCommandValidator : AbstractValidator<AssignTechnicianCommand>
{
    public AssignTechnicianCommandValidator()
    {
        RuleFor(x => x.ServiceOrderId)
            .NotEmpty();

        RuleFor(x => x.TechnicianId)
            .NotEmpty();
    }
}
