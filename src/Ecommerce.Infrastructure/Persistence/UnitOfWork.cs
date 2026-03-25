using Ecommerce.Application.Common.Interfaces;

namespace Ecommerce.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context, IProductRepository products, IOrderRepository orders, ICartRepository carts)
    {
        _context = context;
        Products = products;
        Orders = orders;
        Carts = carts;
    }

    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }
    public ICartRepository Carts { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
