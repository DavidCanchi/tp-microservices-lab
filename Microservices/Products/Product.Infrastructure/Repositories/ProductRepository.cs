using Microsoft.EntityFrameworkCore;
using Product.Domain.Repositories;
using Product.Infrastructure.Data;

namespace Product.Infrastructure.Repositories;

using ProductEntity = global::Product.Domain.Models.Entities.Product;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductEntity>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<ProductEntity?> GetByIdAsync(int id)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ProductEntity> AddAsync(ProductEntity product)
    {
        await _context.Products.AddAsync(product);
        return product;
    }

    public async Task<ProductEntity> UpdateAsync(ProductEntity product)
    {
        _context.Products.Update(product);
        return await Task.FromResult(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await GetByIdAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        return true;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
