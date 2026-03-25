namespace Ecommerce.API.Models.Products;

public record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Status,
    DateTime CreatedAt);
