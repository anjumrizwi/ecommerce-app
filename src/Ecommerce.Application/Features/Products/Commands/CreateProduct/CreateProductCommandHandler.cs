using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = Product.Create(
            command.Name,
            command.Description,
            command.Price,
            command.StockQuantity);

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
