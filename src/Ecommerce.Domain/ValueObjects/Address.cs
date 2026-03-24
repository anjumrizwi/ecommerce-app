namespace Ecommerce.Domain.ValueObjects;

public sealed class Address : IEquatable<Address>
{
    public string Line1 { get; }
    public string? Line2 { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(
        string line1,
        string city,
        string state,
        string postalCode,
        string country,
        string? line2 = null)
    {
        Line1 = Normalize(line1, nameof(line1));
        City = Normalize(city, nameof(city));
        State = Normalize(state, nameof(state));
        PostalCode = Normalize(postalCode, nameof(postalCode));
        Country = Normalize(country, nameof(country));
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
    }

    private static string Normalize(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} is required.", paramName);

        return value.Trim();
    }

    public bool Equals(Address? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(Line1, other.Line1, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Line2, other.Line2, StringComparison.OrdinalIgnoreCase)
            && string.Equals(City, other.City, StringComparison.OrdinalIgnoreCase)
            && string.Equals(State, other.State, StringComparison.OrdinalIgnoreCase)
            && string.Equals(PostalCode, other.PostalCode, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Country, other.Country, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as Address);

    public override int GetHashCode() => HashCode.Combine(
        Line1.ToUpperInvariant(),
        Line2?.ToUpperInvariant(),
        City.ToUpperInvariant(),
        State.ToUpperInvariant(),
        PostalCode.ToUpperInvariant(),
        Country.ToUpperInvariant());

    public static bool operator ==(Address? left, Address? right) => Equals(left, right);
    public static bool operator !=(Address? left, Address? right) => !Equals(left, right);

    public override string ToString() =>
        Line2 is null
            ? $"{Line1}, {City}, {State} {PostalCode}, {Country}"
            : $"{Line1}, {Line2}, {City}, {State} {PostalCode}, {Country}";
}
