namespace Ecommerce.Application.Services.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Status,
    DateTime CreatedAt);

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);

public record PagedProductsRequest(int PageNumber = 1, int PageSize = 20);
