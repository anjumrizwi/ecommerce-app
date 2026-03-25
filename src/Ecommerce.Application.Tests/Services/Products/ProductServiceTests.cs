using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Services.Products;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Ecommerce.Application.Tests.Services.Products;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);

        _sut = new ProductService(_unitOfWorkMock.Object);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_WhenProductsExist_ReturnsAllMappedDtos()
    {
        // Arrange
        var products = new List<Product>
        {
            Product.Create("Widget A", "A great widget", 9.99m, 100),
            Product.Create("Widget B", "Another widget", 19.99m, 50),
        };

        _productRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Widget A" && p.Price == 9.99m && p.StockQuantity == 100);
        result.Should().Contain(p => p.Name == "Widget B" && p.Price == 19.99m && p.StockQuantity == 50);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoProductsExist_ReturnsEmptyCollection()
    {
        // Arrange
        _productRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Product>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_MapsStatusToString()
    {
        // Arrange
        var product = Product.Create("Widget", "Desc", 5m, 10);
        _productRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { product });

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Single().Status.Should().Be("Active");
    }

    [Fact]
    public async Task GetAllAsync_PassesCancellationTokenToRepository()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _productRepositoryMock
            .Setup(r => r.GetAllAsync(token))
            .ReturnsAsync(Enumerable.Empty<Product>());

        // Act
        await _sut.GetAllAsync(token);

        // Assert
        _productRepositoryMock.Verify(r => r.GetAllAsync(token), Times.Once);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsMappedDto()
    {
        // Arrange
        var product = Product.Create("Widget A", "A great widget", 9.99m, 100);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetByIdAsync(product.Id);

        // Assert
        result.Id.Should().Be(product.Id);
        result.Name.Should().Be("Widget A");
        result.Description.Should().Be("A great widget");
        result.Price.Should().Be(9.99m);
        result.StockQuantity.Should().Be(100);
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.GetByIdAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Product*{id}*");
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyGuid_ThrowsNotFoundException()
    {
        // Arrange
        var emptyId = Guid.Empty;

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(emptyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.GetByIdAsync(emptyId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_PassesCancellationTokenToRepository()
    {
        // Arrange
        var product = Product.Create("Widget", "Desc", 5m, 10);
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, token))
            .ReturnsAsync(product);

        // Act
        await _sut.GetByIdAsync(product.Id, token);

        // Assert
        _productRepositoryMock.Verify(r => r.GetByIdAsync(product.Id, token), Times.Once);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsNewProductId()
    {
        // Arrange
        var request = new CreateProductRequest("New Widget", "Some description", 29.99m, 200);

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_AddsProductToRepository()
    {
        // Arrange
        var request = new CreateProductRequest("New Widget", "Some description", 29.99m, 200);

        Product? capturedProduct = null;

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => capturedProduct = p)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.CreateAsync(request);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct!.Name.Should().Be("New Widget");
        capturedProduct.Description.Should().Be("Some description");
        capturedProduct.Price.Should().Be(29.99m);
        capturedProduct.StockQuantity.Should().Be(200);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_SavesChanges()
    {
        // Arrange
        var request = new CreateProductRequest("Widget", "Desc", 5m, 10);

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.CreateAsync(request);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithZeroPrice_CreatesProductSuccessfully()
    {
        // Arrange
        var request = new CreateProductRequest("Free Widget", "Free stuff", 0m, 1);

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_WithZeroStock_CreatesProductSuccessfully()
    {
        // Arrange
        var request = new CreateProductRequest("Out of stock", "Coming soon", 9.99m, 0);

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenProductExists_UpdatesProduct()
    {
        // Arrange
        var product = Product.Create("Old Name", "Old Desc", 1m, 5);
        var request = new UpdateProductRequest("New Name", "New Desc", 99.99m, 50);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(product.Id, request);

        // Assert
        product.Name.Should().Be("New Name");
        product.Description.Should().Be("New Desc");
        product.Price.Should().Be(99.99m);
        product.StockQuantity.Should().Be(50);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_SavesChanges()
    {
        // Arrange
        var product = Product.Create("Widget", "Desc", 5m, 10);
        var request = new UpdateProductRequest("Updated", "Updated Desc", 10m, 20);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.UpdateAsync(product.Id, request);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateProductRequest("Name", "Desc", 10m, 5);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.UpdateAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Product*{id}*");
    }

    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_DoesNotSaveChanges()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateProductRequest("Name", "Desc", 10m, 5);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.UpdateAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenProductExists_DeletesProduct()
    {
        // Arrange
        var product = Product.Create("Widget", "Desc", 5m, 10);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(product.Id);

        // Assert
        _productRepositoryMock.Verify(r => r.Delete(product), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_SavesChanges()
    {
        // Arrange
        var product = Product.Create("Widget", "Desc", 5m, 10);

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(product.Id);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Product*{id}*");
    }

    [Fact]
    public async Task DeleteAsync_WhenProductDoesNotExist_DoesNotCallDelete()
    {
        // Arrange
        var id = Guid.NewGuid();

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _productRepositoryMock.Verify(r => r.Delete(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductDoesNotExist_DoesNotSaveChanges()
    {
        // Arrange
        var id = Guid.NewGuid();

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
