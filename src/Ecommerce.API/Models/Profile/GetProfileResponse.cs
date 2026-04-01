namespace Ecommerce.API.Models.Profile;

public sealed record GetProfileResponse(
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string? PhysicalAddress,
    string? PinCode,
    string? Country,
    string? State,
    string? GoogleMapLink);
