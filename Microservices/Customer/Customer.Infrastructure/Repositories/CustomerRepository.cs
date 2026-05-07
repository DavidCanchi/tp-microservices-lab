using Microsoft.EntityFrameworkCore;
using Customer.Domain.Models.Entities;
using Customer.Domain.Repositories;
using Customer.Infrastructure.Data;

namespace Customer.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _context;

    public CustomerRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<global::Customer.Domain.Models.Entities.Customer>> GetAllAsync()
    {
        return await _context.Customers.ToListAsync();
    }

    public async Task<global::Customer.Domain.Models.Entities.Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<global::Customer.Domain.Models.Entities.Customer> AddAsync(global::Customer.Domain.Models.Entities.Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        return customer;
    }

    public async Task<global::Customer.Domain.Models.Entities.Customer> UpdateAsync(global::Customer.Domain.Models.Entities.Customer customer)
    {
        _context.Customers.Update(customer);
        return await Task.FromResult(customer);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await GetByIdAsync(id);
        if (customer == null)
            return false;

        _context.Customers.Remove(customer);
        return true;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}