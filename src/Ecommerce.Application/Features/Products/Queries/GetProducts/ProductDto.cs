namespace Ecommerce.Application.Features.Products.Queries.GetProducts;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Status,
    DateTime CreatedAt);
