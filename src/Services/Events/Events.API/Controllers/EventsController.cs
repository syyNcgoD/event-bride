using Events.Application.Commands.Events;
using Events.Application.Common.Models;
using Events.Application.DTOs;
using Events.Application.Queries.Events;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EventSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool upcomingOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEventsQuery(page, pageSize, upcomingOnly);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("featured")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<EventSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatured(
        [FromQuery] int count = 5,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFeaturedEventsQuery(count);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var query = new GetEventByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("organizer/{organizerId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<EventSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrganizer(string organizerId, CancellationToken cancellationToken)
    {
        var query = new GetEventsByOrganizerQuery(organizerId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EventResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        var organizerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(organizerId))
        {
            return Unauthorized();
        }

        var command = new CreateEventCommand(
            request.Title,
            request.Description,
            request.ImageUrl,
            request.VenueId,
            request.CategoryId,
            organizerId,
            request.StartDate,
            request.EndDate,
            request.DoorsOpen,
            request.TicketTypes);

        var result = await _mediator.Send(command, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateEventRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEventCommand(
            id,
            request.Title,
            request.Description,
            request.ImageUrl,
            request.VenueId,
            request.CategoryId,
            request.StartDate,
            request.EndDate,
            request.DoorsOpen,
            request.Status,
            request.IsFeatured);

        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteEventCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
