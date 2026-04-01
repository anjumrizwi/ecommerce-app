namespace Ecommerce.API.Models.Profile;

public sealed record UpdateProfileRequest(
    string? PhysicalAddress,
    string? PinCode,
    string? Country,
    string? State,
    string? GoogleMapLink);
