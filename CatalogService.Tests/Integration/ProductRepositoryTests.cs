using CatalogService.DAL.Context;
using CatalogService.DAL.Repositories;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests.Integration;

public class ProductRepositoryTests : IDisposable
{
    private readonly CatalogDbContext _context;
    private readonly ProductRepository _sut;
    private readonly Category _category;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CatalogDbContext(options);
        _sut = new ProductRepository(_context);

        _category = new Category { Name = "Test Category" };
        _context.Categories.Add(_category);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_RoundtripsProduct()
    {
        var product = new Product
        {
            Name = "Test Laptop",
            Description = "<b>Fast</b>",
            Price = 999m,
            Amount = 5,
            CategoryId = _category.Id
        };

        await _sut.AddAsync(product);
        var loaded = await _sut.GetByIdAsync(product.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Test Laptop", loaded.Name);
        Assert.Equal("<b>Fast</b>", loaded.Description);
        Assert.Equal(999m, loaded.Price);
        Assert.Equal(5, loaded.Amount);
        Assert.Equal(_category.Id, loaded.CategoryId);
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsOnlyProductsInCategory()
    {
        var otherCategory = new Category { Name = "Other" };
        _context.Categories.Add(otherCategory);
        await _context.SaveChangesAsync();

        await _sut.AddAsync(new Product { Name = "P1", Price = 10m, Amount = 1, CategoryId = _category.Id });
        await _sut.AddAsync(new Product { Name = "P2", Price = 20m, Amount = 2, CategoryId = otherCategory.Id });

        var result = (await _sut.GetByCategoryAsync(_category.Id)).ToList();

        Assert.Single(result);
        Assert.Equal("P1", result[0].Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var product = new Product { Name = "ToRemove", Price = 1m, Amount = 1, CategoryId = _category.Id };
        await _sut.AddAsync(product);

        await _sut.DeleteAsync(product.Id);

        var loaded = await _sut.GetByIdAsync(product.Id);
        Assert.Null(loaded);
    }

    public void Dispose() => _context.Dispose();
}
