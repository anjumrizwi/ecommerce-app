using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Cart : BaseEntity
{
    private const int MaxCartItems = 100;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    public static Cart Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        return new Cart { UserId = userId };
    }

    public void AddItem(Guid productId, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        var existing = _items.FirstOrDefault(x => x.ProductId == productId);
        if (existing is not null)
        {
            // Check for integer overflow
            if (existing.Quantity > int.MaxValue - quantity)
                throw new InvalidOperationException("Adding this quantity would exceed the maximum allowed.");

            existing.SetQuantity(existing.Quantity + quantity);
            return;
        }

        // Check max cart items limit
        if (_items.Count >= MaxCartItems)
            throw new InvalidOperationException($"Cannot add more than {MaxCartItems} different items to cart.");

        _items.Add(CartItem.Create(Id, productId, quantity));
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item is null) return;

        if (quantity <= 0)
        {
            _items.Remove(item);
            return;
        }

        item.SetQuantity(quantity);
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item is not null) _items.Remove(item);
    }

    public void Clear() => _items.Clear();
}
