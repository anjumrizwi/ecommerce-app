using Ecommerce.Application.Services.Carts;

namespace Ecommerce.Application.Common.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddItemAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Guid> CheckoutAsync(Guid userId, CancellationToken cancellationToken = default);
}
