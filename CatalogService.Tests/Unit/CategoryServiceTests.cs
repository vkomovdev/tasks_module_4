using CatalogService.BLL.DTOs;
using CatalogService.BLL.Services;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;
using Moq;

namespace CatalogService.Tests.Unit;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repoMock = new();
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_repoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCategoryDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMappedDto_WhenCategoryExists()
    {
        var category = new Category { Id = 1, Name = "Electronics", ImageUrl = "https://example.com/el.png" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Electronics", result.Name);
        Assert.Equal("https://example.com/el.png", result.ImageUrl);
        Assert.Null(result.ParentCategoryId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics" },
            new() { Id = 2, Name = "Books" }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = (await _sut.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.Name == "Electronics");
        Assert.Contains(result, d => d.Name == "Books");
    }

    [Fact]
    public async Task AddAsync_PersistsAndReturnsDto()
    {
        var dto = new CreateCategoryDto { Name = "Clothing", ImageUrl = null, ParentCategoryId = null };
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => { c.Id = 5; return c; });

        var result = await _sut.AddAsync(dto);

        Assert.Equal(5, result.Id);
        Assert.Equal("Clothing", result.Name);
        _repoMock.Verify(r => r.AddAsync(It.Is<Category>(c => c.Name == "Clothing")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenCategoryMissing()
    {
        _repoMock.Setup(r => r.GetByIdAsync(404)).ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateAsync(404, new UpdateCategoryDto { Name = "X" }));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        var existing = new Category { Id = 3, Name = "Old Name" };
        _repoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(existing);

        await _sut.UpdateAsync(3, new UpdateCategoryDto { Name = "New Name", ImageUrl = "https://img.com/x.png" });

        _repoMock.Verify(r => r.UpdateAsync(It.Is<Category>(c =>
            c.Name == "New Name" &&
            c.ImageUrl == "https://img.com/x.png")), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DelegatestoRepository()
    {
        await _sut.DeleteAsync(7);
        _repoMock.Verify(r => r.DeleteAsync(7), Times.Once);
    }
}
