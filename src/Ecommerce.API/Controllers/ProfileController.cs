using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.API.Models.Profile;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return NotFound(new { message = "User profile not found." });

        return Ok(new GetProfileResponse(
            user.Id.ToString(),
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role.ToString(),
            user.PhysicalAddress,
            user.PinCode,
            user.Country,
            user.State,
            user.GoogleMapLink));
    }

    [HttpPut]
    [ProducesResponseType(typeof(GetProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        Uri? parsedUri = null;

        if (!string.IsNullOrWhiteSpace(request.GoogleMapLink)
            && !Uri.TryCreate(request.GoogleMapLink, UriKind.Absolute, out parsedUri))
        {
            return BadRequest(new { message = "Google Map link must be a valid absolute URL." });
        }

        if (!string.IsNullOrWhiteSpace(request.GoogleMapLink)
            && parsedUri is not null
            && parsedUri.Scheme != Uri.UriSchemeHttps
            && parsedUri.Scheme != Uri.UriSchemeHttp)
        {
            return BadRequest(new { message = "Google Map link must use http or https." });
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return NotFound(new { message = "User profile not found." });

        user.UpdateProfile(
            request.PhysicalAddress,
            request.PinCode,
            request.Country,
            request.State,
            request.GoogleMapLink);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new GetProfileResponse(
            user.Id.ToString(),
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role.ToString(),
            user.PhysicalAddress,
            user.PinCode,
            user.Country,
            user.State,
            user.GoogleMapLink));
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
