namespace Ecommerce.Application.Services.Carts;

public record CheckoutRequest(
    string PaymentMethod,
    string? PaymentReference);

public record CheckoutResult(
    Guid OrderId,
    string PaymentMethod,
    string PaymentStatus);

public record CartDto(
    Guid Id,
    Guid UserId,
    IEnumerable<CartItemDto> Items,
    decimal TotalAmount);

public record CartItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);
