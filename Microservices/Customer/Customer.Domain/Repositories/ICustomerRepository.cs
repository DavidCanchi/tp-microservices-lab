namespace Customer.Domain.Repositories;

public interface ICustomerRepository
{
    Task<IEnumerable<global::Customer.Domain.Models.Entities.Customer>> GetAllAsync();
    Task<global::Customer.Domain.Models.Entities.Customer?> GetByIdAsync(int id);
    Task<global::Customer.Domain.Models.Entities.Customer> AddAsync(global::Customer.Domain.Models.Entities.Customer customer);
    Task<global::Customer.Domain.Models.Entities.Customer> UpdateAsync(global::Customer.Domain.Models.Entities.Customer customer);
    Task<bool> DeleteAsync(int id);
    Task SaveChangesAsync();
}
