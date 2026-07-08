using Application.ServiceOrders.Commands.AssignTechnician;
using Application.ServiceOrders.Commands.CreateServiceOrder;
using Application.ServiceOrders.Queries.GetOrdersByStatus;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/service-orders")]
public class ServiceOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiceOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByStatus(
        [FromQuery] OrderStatus status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrdersByStatusQuery(status), cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateServiceOrderResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateServiceOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}/technician")]
    [ProducesResponseType(typeof(AssignTechnicianResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTechnician(
        Guid id,
        [FromBody] AssignTechnicianRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(
                new AssignTechnicianCommand(id, request.TechnicianId),
                cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

/// <summary>
/// Request body for assigning a technician to a service order.
/// </summary>
public record AssignTechnicianRequest(Guid TechnicianId);
