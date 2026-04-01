using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.API.Models.Orders;
using Ecommerce.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppOrders = Ecommerce.Application.Services.Orders;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    /// <summary>Gets all orders for the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var orders = await orderService.GetByCustomerIdAsync(userId.ToString(), cancellationToken);
        return Ok(orders.Select(MapToResponse));
    }

    /// <summary>Gets a single order for the authenticated user.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var order = await orderService.GetByIdAsync(id, cancellationToken);

        if (!string.Equals(order.CustomerId, userId.ToString(), StringComparison.OrdinalIgnoreCase))
            return NotFound();

        return Ok(MapToResponse(order));
    }

    /// <summary>Marks an authenticated user's order as delivered.</summary>
    [HttpPatch("{id:guid}/deliver")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsDelivered(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var order = await orderService.GetByIdAsync(id, cancellationToken);

        if (!string.Equals(order.CustomerId, userId.ToString(), StringComparison.OrdinalIgnoreCase))
            return NotFound();

        await orderService.MarkAsDeliveredAsync(id, cancellationToken);
        return NoContent();
    }

    private static OrderResponse MapToResponse(AppOrders.OrderDto order) => new(
        order.Id,
        order.CustomerId,
        order.Status,
        order.PaymentMethod,
        order.PaymentStatus,
        order.PaymentReference,
        order.TotalAmount,
        order.Items.Select(item => new OrderItemResponse(
            item.ProductId,
            item.ProductName,
            item.UnitPrice,
            item.Quantity,
            item.TotalPrice)),
        order.CreatedAt);

    private Guid GetUserId()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("nameid")?.Value;

        if (!Guid.TryParse(userId, out var parsed))
            throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");

        return parsed;
    }
}
