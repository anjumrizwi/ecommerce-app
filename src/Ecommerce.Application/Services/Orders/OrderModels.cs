namespace Ecommerce.Application.Services.Orders;

public record OrderDto(
    Guid Id,
    string CustomerId,
    string Status,
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
    IEnumerable<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity);
