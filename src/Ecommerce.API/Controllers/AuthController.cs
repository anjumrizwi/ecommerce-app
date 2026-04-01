using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.API.Models.Auth;
using Ecommerce.API.Security;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DomainUser = Ecommerce.Domain.Entities.User;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;

    public AuthController(AppDbContext db, IJwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return BadRequest(new { message = "First name is required." });

        if (string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new { message = "Last name is required." });

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required." });

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Password is required." });

        if (request.Password.Length < 8)
            return BadRequest(new { message = "Password must be at least 8 characters long." });

        // Validate email format
        if (!IsValidEmail(request.Email))
            return BadRequest(new { message = "Invalid email format." });

        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email, cancellationToken))
            return Conflict(new { message = "Email already registered." });

        var role = Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var parsed)
            ? parsed
            : UserRole.Customer;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var newUser = DomainUser.Create(request.FirstName, request.LastName, email, passwordHash, role);

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync(cancellationToken);

        var token = _jwt.GenerateToken(newUser);
        return Ok(new AuthResponse(token, newUser.Email, newUser.FirstName, newUser.LastName, newUser.Role.ToString()));
    }

    /// <summary>Login with email and password.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email is required." });

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Password is required." });

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials." });

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Email, user.FirstName, user.LastName, user.Role.ToString()));
    }

    /// <summary>Returns the currently authenticated user's identity from the JWT claims.</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("nameid")?.Value;
        var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var firstName = User.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value
            ?? User.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = User.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value
            ?? User.FindFirst(ClaimTypes.Surname)?.Value;
        var displayName = BuildDisplayName(firstName, lastName);

        return Ok(new { userId, email, role, displayName });
    }

    /// <summary>Admin-only endpoint to verify role-based authorization.</summary>
    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly() =>
        Ok(new { message = "You have Admin access." });

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static string? BuildDisplayName(string? firstName, string? lastName)
    {
        var normalizedFirstName = firstName?.Trim();
        var normalizedLastName = lastName?.Trim();

        if (string.IsNullOrEmpty(normalizedFirstName) && string.IsNullOrEmpty(normalizedLastName))
            return null;

        if (string.IsNullOrEmpty(normalizedFirstName))
            return normalizedLastName?.ToLowerInvariant();

        if (string.IsNullOrEmpty(normalizedLastName))
            return normalizedFirstName.ToLowerInvariant();

        return string.Concat(normalizedFirstName[0], normalizedLastName).ToLowerInvariant();
    }
}
