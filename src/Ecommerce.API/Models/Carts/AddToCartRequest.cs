namespace Ecommerce.API.Models.Carts;

public record AddToCartRequest(
    Guid ProductId,
    int Quantity);
