using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Common.Interfaces;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
