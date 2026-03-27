using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

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

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Drop stale tracked state so callers can safely reload and retry.
            _context.ChangeTracker.Clear();
            throw new ConflictException("The data was modified by another operation. Please retry.");
        }
    }
}
