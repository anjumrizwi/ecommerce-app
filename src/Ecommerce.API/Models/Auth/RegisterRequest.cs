namespace Ecommerce.API.Models.Auth;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role = "Customer");
