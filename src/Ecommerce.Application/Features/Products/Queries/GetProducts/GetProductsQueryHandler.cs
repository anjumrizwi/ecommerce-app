using Ecommerce.Application.Features.Products.Queries.GetProducts;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler(IProductRepository productRepository)
{
    public async Task<IEnumerable<ProductDto>> Handle(CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        return products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.StockQuantity,
            p.Status.ToString(),
            p.CreatedAt));
    }
}
