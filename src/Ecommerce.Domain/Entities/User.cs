using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? PhysicalAddress { get; private set; }
    public string? PinCode { get; private set; }
    public string? Country { get; private set; }
    public string? State { get; private set; }
    public string? GoogleMapLink { get; private set; }
    public UserRole Role { get; private set; } = UserRole.Customer;

    public Cart? Cart { get; private set; }

    private User() { }

    public static User Create(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        UserRole role = UserRole.Customer)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role
        };
    }

    public void UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRole(UserRole role)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(
        string? physicalAddress,
        string? pinCode,
        string? country,
        string? state,
        string? googleMapLink)
    {
        PhysicalAddress = string.IsNullOrWhiteSpace(physicalAddress)
            ? null
            : physicalAddress.Trim();

        PinCode = string.IsNullOrWhiteSpace(pinCode)
            ? null
            : pinCode.Trim();

        Country = string.IsNullOrWhiteSpace(country)
            ? null
            : country.Trim();

        State = string.IsNullOrWhiteSpace(state)
            ? null
            : state.Trim();

        GoogleMapLink = string.IsNullOrWhiteSpace(googleMapLink)
            ? null
            : googleMapLink.Trim();

        UpdatedAt = DateTime.UtcNow;
    }
}
