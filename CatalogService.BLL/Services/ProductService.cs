using CatalogService.BLL.DTOs;
using CatalogService.BLL.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Interfaces;

namespace CatalogService.BLL.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product is null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await _repository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId)
    {
        var products = await _repository.GetByCategoryAsync(categoryId);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> AddAsync(CreateProductDto dto)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(dto));
        if (dto.Price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(dto));

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            Price = dto.Price,
            Amount = dto.Amount
        };
        var created = await _repository.AddAsync(product);
        return MapToDto(created);
    }

    public async Task UpdateAsync(int id, UpdateProductDto dto)
    {
        if (dto.Amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(dto));
        if (dto.Price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(dto));

        var product = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.ImageUrl = dto.ImageUrl;
        product.CategoryId = dto.CategoryId;
        product.Price = dto.Price;
        product.Amount = dto.Amount;
        await _repository.UpdateAsync(product);
    }

    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        ImageUrl = p.ImageUrl,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        Price = p.Price,
        Amount = p.Amount
    };
}
