namespace Ecommerce.API.Models.Products;

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);
