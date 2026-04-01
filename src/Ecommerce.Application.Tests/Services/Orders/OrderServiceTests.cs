using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Services.Orders;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Ecommerce.Application.Tests.Services.Orders;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepositoryMock = new Mock<IOrderRepository>();

        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepositoryMock.Object);

        _sut = new OrderService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task MarkAsDeliveredAsync_WhenOrderUsesCashOnDelivery_SetsPaymentStatusToPaid()
    {
        // Arrange
        var order = BuildConfirmedOrder("buyer-1", "CashOnDelivery");

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.MarkAsDeliveredAsync(order.Id, CancellationToken.None);

        // Assert
        order.Status.ToString().Should().Be("Delivered");
        order.PaymentMethod.ToString().Should().Be("CashOnDelivery");
        order.PaymentStatus.ToString().Should().Be("Paid");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsDeliveredAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var act = () => _sut.MarkAsDeliveredAsync(orderId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Order BuildConfirmedOrder(string customerId, string paymentMethod)
    {
        var product = Product.Create("Phone", "Flagship phone", 499m, 5);
        var order = Order.Create(customerId);
        order.AddItem(product, 1);
        order.SetPayment(Enum.Parse<PaymentMethod>(paymentMethod, ignoreCase: true));
        order.Confirm();
        return order;
    }
}
