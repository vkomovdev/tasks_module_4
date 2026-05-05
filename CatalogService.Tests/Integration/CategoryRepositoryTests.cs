using CatalogService.DAL.Context;
using CatalogService.DAL.Repositories;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests.Integration;

public class CategoryRepositoryTests : IDisposable
{
    private readonly CatalogDbContext _context;
    private readonly CategoryRepository _sut;

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CatalogDbContext(options);
        _sut = new CategoryRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_RoundtripsCategory()
    {
        var category = new Category { Name = "Electronics" };

        await _sut.AddAsync(category);
        var loaded = await _sut.GetByIdAsync(category.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Electronics", loaded.Name);
        Assert.Null(loaded.ParentCategoryId);
    }

    [Fact]
    public async Task AddAsync_SetsParentCategory()
    {
        var parent = new Category { Name = "Tech" };
        await _sut.AddAsync(parent);

        var child = new Category { Name = "Laptops", ParentCategoryId = parent.Id };
        await _sut.AddAsync(child);

        var loaded = await _sut.GetByIdAsync(child.Id);

        Assert.Equal(parent.Id, loaded!.ParentCategoryId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        await _sut.AddAsync(new Category { Name = "A" });
        await _sut.AddAsync(new Category { Name = "B" });

        var all = (await _sut.GetAllAsync()).ToList();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var category = new Category { Name = "Old" };
        await _sut.AddAsync(category);

        category.Name = "Updated";
        await _sut.UpdateAsync(category);

        var loaded = await _sut.GetByIdAsync(category.Id);
        Assert.Equal("Updated", loaded!.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategory()
    {
        var category = new Category { Name = "ToDelete" };
        await _sut.AddAsync(category);

        await _sut.DeleteAsync(category.Id);

        var loaded = await _sut.GetByIdAsync(category.Id);
        Assert.Null(loaded);
    }

    [Fact]
    public async Task DeleteAsync_IsIdempotent_WhenCategoryDoesNotExist()
    {
        // Should not throw
        await _sut.DeleteAsync(9999);
    }

    public void Dispose() => _context.Dispose();
}
