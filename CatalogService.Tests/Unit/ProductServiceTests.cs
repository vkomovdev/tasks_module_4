using CatalogService.BLL.DTOs;
using CatalogService.BLL.Services;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using Moq;

namespace CatalogService.Tests.Unit;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repoMock.Object);
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentException_WhenAmountIsZero()
    {
        var dto = new CreateProductDto { Name = "Widget", CategoryId = 1, Price = 10m, Amount = 0 };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddAsync(dto));
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentException_WhenAmountIsNegative()
    {
        var dto = new CreateProductDto { Name = "Widget", CategoryId = 1, Price = 10m, Amount = -1 };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddAsync(dto));
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentException_WhenPriceIsNegative()
    {
        var dto = new CreateProductDto { Name = "Widget", CategoryId = 1, Price = -5m, Amount = 1 };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddAsync(dto));
    }

    [Fact]
    public async Task AddAsync_PersistsAndReturnsDto()
    {
        var dto = new CreateProductDto
        {
            Name = "Laptop Pro",
            Description = "<b>Powerful</b>",
            CategoryId = 2,
            Price = 1299.99m,
            Amount = 10
        };
        var category = new Category { Id = 2, Name = "Computers" };
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = 8; p.Category = category; return p; });

        var result = await _sut.AddAsync(dto);

        Assert.Equal(8, result.Id);
        Assert.Equal("Laptop Pro", result.Name);
        Assert.Equal("<b>Powerful</b>", result.Description);
        Assert.Equal(1299.99m, result.Price);
        Assert.Equal(10, result.Amount);
        Assert.Equal("Computers", result.CategoryName);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenProductMissing()
    {
        _repoMock.Setup(r => r.GetByIdAsync(404)).ReturnsAsync((Product?)null);
        var dto = new UpdateProductDto { Name = "X", CategoryId = 1, Price = 1m, Amount = 1 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(404, dto));
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsMappedProducts()
    {
        var cat = new Category { Id = 3, Name = "Audio" };
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Headphones", Price = 79m, Amount = 5, CategoryId = 3, Category = cat },
            new() { Id = 2, Name = "Speaker", Price = 120m, Amount = 3, CategoryId = 3, Category = cat }
        };
        _repoMock.Setup(r => r.GetByCategoryAsync(3)).ReturnsAsync(products);

        var result = (await _sut.GetByCategoryAsync(3)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal("Audio", p.CategoryName));
    }
}
