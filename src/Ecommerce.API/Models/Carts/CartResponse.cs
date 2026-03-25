namespace Ecommerce.API.Models.Carts;

public record CartItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public record CartResponse(
    Guid Id,
    Guid UserId,
    IEnumerable<CartItemResponse> Items,
    decimal TotalAmount);
