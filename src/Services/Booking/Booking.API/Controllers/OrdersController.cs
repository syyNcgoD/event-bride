using Booking.Application.Commands.Orders;
using Booking.Application.Common.Models;
using Booking.Application.DTOs;
using Booking.Application.Queries.Orders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var command = new CreateOrderCommand(
            userId,
            request.Email,
            request.PhoneNumber,
            request.Notes,
            request.Items);

        var result = await _mediator.Send(command, cancellationToken);

        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var query = new GetOrderQuery(id, userId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var query = new GetMyOrdersQuery(userId, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:int}/pay")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPayment(
        int id,
        [FromBody] ConfirmPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var command = new ConfirmPaymentCommand(
            id, userId, request.PaymentMethod, request.TransactionId);

        var result = await _mediator.Send(command, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var command = new CancelOrderCommand(id, userId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
    }
}