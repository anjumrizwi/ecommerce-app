using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Cart Cart { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public int Quantity { get; private set; }

    private CartItem() { }

    internal static CartItem Create(Guid cartId, Guid productId, int quantity)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentException("CartId cannot be empty.", nameof(cartId));
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        return new CartItem { CartId = cartId, ProductId = productId, Quantity = quantity };
    }

    internal void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
