using Events.Application.Common.Models;
using Events.Application.DTOs;
using Events.Application.Queries.Tickets;
using Events.Application.Commands.Tickets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.API.Controllers;

[ApiController]
[Route("api/Events/tickets")]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // برای Booking Service: دریافت موجودی و قیمت بلیط
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TicketTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetTicketByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result.Success ? Ok(result.Data) : NotFound(result);
    }

    // رزرو بلیط (کاهش SoldCount) - با قفل Pessimistic در دیتابیس
    [HttpPost("{id:int}/reserve")]
    [AllowAnonymous] // internal service call - in production use service-to-service auth
    public async Task<IActionResult> Reserve(
        int id,
        [FromBody] ReserveTicketsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReserveTicketsCommand(id, request.EventId, request.Quantity);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // آزادسازی بلیط (افزایش SoldCount) - وقتی رزرو منقضی یا لغو می‌شود
    [HttpPost("{id:int}/release")]
    [AllowAnonymous] // internal service call
    public async Task<IActionResult> Release(
        int id,
        [FromBody] ReleaseTicketsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReleaseTicketsCommand(id, request.Quantity);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}

public class ReserveTicketsRequest
{
    public int EventId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class ReleaseTicketsRequest
{
    public int Quantity { get; set; } = 1;
}