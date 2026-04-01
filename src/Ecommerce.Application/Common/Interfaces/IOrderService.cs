using Ecommerce.Application.Services.Orders;

namespace Ecommerce.Application.Common.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task ConfirmAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsDeliveredAsync(Guid id, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
