using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Ecommerce.API.Models.Carts;
using Ecommerce.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController(ICartService cartService, IMapper mapper) : ControllerBase
{
    /// <summary>Gets the authenticated user's cart.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyCart(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var cart = await cartService.GetCartAsync(userId, cancellationToken);
        return Ok(mapper.Map<CartResponse>(cart));
    }

    /// <summary>Adds an item to the authenticated user's cart.</summary>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
            return BadRequest(new { message = "Quantity must be greater than zero." });

        var userId = GetUserId();
        await cartService.AddItemAsync(userId, request.ProductId, request.Quantity, cancellationToken);
        return NoContent();
    }

    /// <summary>Removes an item from the authenticated user's cart.</summary>
    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromCart(Guid productId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await cartService.RemoveItemAsync(userId, productId, cancellationToken);
        return NoContent();
    }

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