namespace Ecommerce.API.Models.Products;

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);
