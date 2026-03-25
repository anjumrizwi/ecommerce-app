using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services.Orders;

public class OrderService(IUnitOfWork unitOfWork) : IOrderService
{
    public async Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await unitOfWork.Orders.GetAllAsync(cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        var orders = await unitOfWork.Orders.GetByCustomerIdAsync(customerId, cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<Guid> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = Order.Create(request.CustomerId);

        var productIds = request.Items.Select(i => i.ProductId).ToHashSet();
        var products = await unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id), cancellationToken);
        var productMap = products.ToDictionary(p => p.Id);

        foreach (var item in request.Items)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
                throw new NotFoundException(nameof(Product), item.ProductId);

            order.AddItem(product, item.Quantity);
        }

        await unitOfWork.Orders.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    public async Task ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Confirm();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Cancel();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static OrderDto MapToDto(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Status.ToString(),
        order.TotalAmount,
        order.Items.Select(i => new OrderItemDto(
            i.ProductId,
            i.ProductName,
            i.UnitPrice,
            i.Quantity,
            i.TotalPrice)),
        order.CreatedAt);
}
