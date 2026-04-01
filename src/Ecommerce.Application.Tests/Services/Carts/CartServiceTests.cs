using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Services.Carts;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Ecommerce.Application.Tests.Services.Carts;

public class CartServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly CartService _sut;

    public CartServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();

        _unitOfWorkMock.Setup(u => u.Carts).Returns(_cartRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepositoryMock.Object);

        _sut = new CartService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task AddItemAsync_WhenProductExists_UsesAtomicRepositoryPath()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Test Description", 10m, 5);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _cartRepositoryMock
            .Setup(r => r.AddItemAtomicAsync(userId, productId, 1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.AddItemAsync(userId, productId, 1, CancellationToken.None);

        // Assert
        _cartRepositoryMock.Verify(r => r.AddItemAtomicAsync(userId, productId, 1, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_WhenProductDoesNotExist_ThrowsNotFoundAndSkipsWrite()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.AddItemAsync(userId, productId, 1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _cartRepositoryMock.Verify(r => r.AddItemAtomicAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_WhenUsingUpi_ReturnsPaidOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var product = Product.Create("Phone", "Flagship phone", 999m, 2);
        var cart = Cart.Create(userId);
        cart.AddItem(product.Id, 1);
        Order? savedOrder = null;

        _cartRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _productRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { product });

        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => savedOrder = order)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CheckoutAsync(
            userId,
            new CheckoutRequest("Upi", "buyer@upi"),
            CancellationToken.None);

        // Assert
        result.PaymentMethod.Should().Be("Upi");
        result.PaymentStatus.Should().Be("Paid");
        savedOrder.Should().NotBeNull();
        savedOrder!.PaymentReference.Should().Be("buyer@upi");
        savedOrder.Status.ToString().Should().Be("Confirmed");
        savedOrder.PaymentStatus.ToString().Should().Be("Paid");
    }

    [Fact]
    public async Task CheckoutAsync_WhenUpiReferenceMissing_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var product = Product.Create("Phone", "Flagship phone", 999m, 2);
        var cart = Cart.Create(userId);
        cart.AddItem(product.Id, 1);

        _cartRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        // Act
        var act = () => _sut.CheckoutAsync(
            userId,
            new CheckoutRequest("Upi", null),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
