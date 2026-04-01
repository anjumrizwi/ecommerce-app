namespace Ecommerce.API.Models.Carts;

public record CheckoutRequest(
    string PaymentMethod,
    string? PaymentReference);