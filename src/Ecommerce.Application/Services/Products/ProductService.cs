using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;

namespace Ecommerce.Application.Services.Products;

public class ProductService(IUnitOfWork unitOfWork, IMemoryCache cache) : IProductService
{
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);
    private const string AllProductsCacheKey = "products:all";
    private static string ProductCacheKey(Guid id) => $"products:{id}";

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(AllProductsCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiry;
            var products = await unitOfWork.Products.GetAllAsync(cancellationToken);
            return products.Select(MapToDto).ToList().AsEnumerable();
        }) ?? Enumerable.Empty<ProductDto>();
    }

    public async Task<PaginatedList<ProductDto>> GetPagedAsync(PagedProductsRequest request, CancellationToken cancellationToken = default)
    {
        var paged = await unitOfWork.Products.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
        var dtos = paged.Items.Select(MapToDto).ToList();
        return new PaginatedList<ProductDto>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(ProductCacheKey(id), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiry;
            var product = await unitOfWork.Products.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), id);
            return MapToDto(product);
        }) ?? throw new NotFoundException(nameof(Product), id);
    }

    public async Task<Guid> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity);

        await unitOfWork.Products.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        cache.Remove(AllProductsCacheKey);

        return product.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        product.Update(request.Name, request.Description, request.Price, request.StockQuantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        cache.Remove(AllProductsCacheKey);
        cache.Remove(ProductCacheKey(id));
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        unitOfWork.Products.Delete(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        cache.Remove(AllProductsCacheKey);
        cache.Remove(ProductCacheKey(id));
    }

    private static ProductDto MapToDto(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.StockQuantity,
        product.Status.ToString(),
        product.CreatedAt);
}
