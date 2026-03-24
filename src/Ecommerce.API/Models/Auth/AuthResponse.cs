namespace Ecommerce.API.Models.Auth;

public sealed record AuthResponse(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    string Role);
