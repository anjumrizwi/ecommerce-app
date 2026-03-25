using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Repositories;

public class OrderRepository(AppDbContext context) : Repository<Order>(context), IOrderRepository
{
    public override async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public override async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Context.Orders
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
        => await Context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(cancellationToken);
}
