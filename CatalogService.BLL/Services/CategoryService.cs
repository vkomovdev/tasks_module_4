using CatalogService.BLL.DTOs;
using CatalogService.BLL.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;

namespace CatalogService.BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category is null ? null : MapToDto(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto> AddAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            ImageUrl = dto.ImageUrl,
            ParentCategoryId = dto.ParentCategoryId
        };
        var created = await _repository.AddAsync(category);
        return MapToDto(created);
    }

    public async Task UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        category.Name = dto.Name;
        category.ImageUrl = dto.ImageUrl;
        category.ParentCategoryId = dto.ParentCategoryId;
        await _repository.UpdateAsync(category);
    }

    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

    private static CategoryDto MapToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        ImageUrl = c.ImageUrl,
        ParentCategoryId = c.ParentCategoryId,
        ParentCategoryName = c.ParentCategory?.Name
    };
}
