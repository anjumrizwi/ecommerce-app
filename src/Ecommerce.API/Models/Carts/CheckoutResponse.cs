namespace Ecommerce.API.Models.Carts;

public record CheckoutResponse(
    Guid OrderId,
    string PaymentMethod,
    string PaymentStatus);