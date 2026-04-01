namespace Ecommerce.Application.Services.Orders;

public record OrderDto(
    Guid Id,
    string CustomerId,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    string? PaymentReference,
    decimal TotalAmount,
    IEnumerable<OrderItemDto> Items,
    DateTime CreatedAt);

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public record CreateOrderRequest(
    string CustomerId,
    string PaymentMethod,
    string? PaymentReference,
    IEnumerable<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity);
