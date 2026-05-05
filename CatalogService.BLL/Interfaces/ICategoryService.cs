using CatalogService.BLL.DTOs;

namespace CatalogService.BLL.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto> AddAsync(CreateCategoryDto dto);
    Task UpdateAsync(int id, UpdateCategoryDto dto);
    Task DeleteAsync(int id);
}
