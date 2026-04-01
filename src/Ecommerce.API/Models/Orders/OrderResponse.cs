namespace Ecommerce.API.Models.Orders;

public record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public record OrderResponse(
    Guid Id,
    string CustomerId,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    string? PaymentReference,
    decimal TotalAmount,
    IEnumerable<OrderItemResponse> Items,
    DateTime CreatedAt);
