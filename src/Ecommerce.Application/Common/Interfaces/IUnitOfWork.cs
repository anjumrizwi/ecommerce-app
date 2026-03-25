namespace Ecommerce.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    ICartRepository Carts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
