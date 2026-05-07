namespace Product.Domain.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<global::Product.Domain.Models.Entities.Product>> GetAllAsync();
    Task<global::Product.Domain.Models.Entities.Product?> GetByIdAsync(int id);
    Task<global::Product.Domain.Models.Entities.Product> AddAsync(global::Product.Domain.Models.Entities.Product product);
    Task<global::Product.Domain.Models.Entities.Product> UpdateAsync(global::Product.Domain.Models.Entities.Product product);
    Task<bool> DeleteAsync(int id);
    Task SaveChangesAsync();
}
