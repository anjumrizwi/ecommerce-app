using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services.Carts;

public class CartService(IUnitOfWork unitOfWork) : ICartService
{
    public async Task<CartDto> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await unitOfWork.Carts.GetByUserIdAsync(userId, cancellationToken);

        return cart is null
            ? new CartDto(Guid.Empty, userId, [], 0m)
            : MapToDto(cart);
    }

    public async Task AddItemAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        _ = await unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), productId);

        var cart = await unitOfWork.Carts.GetByUserIdAsync(userId, cancellationToken);
        if (cart is null)
        {
            cart = Cart.Create(userId);
            await unitOfWork.Carts.AddAsync(cart, cancellationToken);
        }

        cart.AddItem(productId, quantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateItemQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var cart = await unitOfWork.Carts.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Cart), userId);

        cart.UpdateItemQuantity(productId, quantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var cart = await unitOfWork.Carts.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Cart), userId);

        cart.RemoveItem(productId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await unitOfWork.Carts.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Cart), userId);

        cart.Clear();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> CheckoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cart = await unitOfWork.Carts.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Cart), userId);

        if (!cart.Items.Any())
            throw new InvalidOperationException("Cannot checkout an empty cart.");

        var productIds = cart.Items.Select(i => i.ProductId).ToHashSet();
        var products = await unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id), cancellationToken);
        var productMap = products.ToDictionary(p => p.Id);

        var order = Order.Create(userId.ToString());

        foreach (var item in cart.Items)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
                throw new NotFoundException(nameof(Product), item.ProductId);

            order.AddItem(product, item.Quantity);
        }

        await unitOfWork.Orders.AddAsync(order, cancellationToken);
        cart.Clear();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items.Select(i => new CartItemDto(
            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Product?.Price ?? 0m,
            i.Quantity,
            (i.Product?.Price ?? 0m) * i.Quantity));

        return new CartDto(
            cart.Id,
            cart.UserId,
            items,
            items.Sum(i => i.TotalPrice));
    }
}
