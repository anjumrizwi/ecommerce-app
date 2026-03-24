using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = new();

    public string CustomerId { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount => _items.Sum(i => i.TotalPrice);
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerId is required.", nameof(customerId));

        return new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Pending
        };
    }

    public void AddItem(Product product, int quantity)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot add items to an order with status {Status}.");

        var item = OrderItem.Create(Id, product.Id, product.Name, product.Price, quantity);
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm an order with status {Status}.");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm an order with no items.");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled.");

        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel a delivered order.");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
