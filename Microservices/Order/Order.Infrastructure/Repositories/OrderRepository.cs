using Microsoft.EntityFrameworkCore;
using Order.Domain.Repositories;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories;

using OrderEntity = global::Order.Domain.Models.Entities.Order;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderEntity>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ToListAsync();
    }

    public async Task<OrderEntity?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<OrderEntity>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<OrderEntity> AddAsync(OrderEntity order)
    {
        await _context.Orders.AddAsync(order);
        return order;
    }

    public async Task<OrderEntity> UpdateAsync(OrderEntity order)
    {
        _context.Orders.Update(order);
        return await Task.FromResult(order);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await GetByIdAsync(id);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        return true;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
