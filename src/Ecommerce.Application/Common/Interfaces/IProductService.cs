using Ecommerce.Application.Services.Products;
using Ecommerce.Application.Common.Models;

namespace Ecommerce.Application.Common.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaginatedList<ProductDto>> GetPagedAsync(PagedProductsRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
