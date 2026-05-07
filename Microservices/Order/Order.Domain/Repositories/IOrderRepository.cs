namespace Order.Domain.Repositories;

using OrderEntity = global::Order.Domain.Models.Entities.Order;

public interface IOrderRepository
{
    /// <summary>
    /// Obtiene todas las órdenes
    /// </summary>
    Task<IEnumerable<OrderEntity>> GetAllAsync();

    /// <summary>
    /// Obtiene una orden por su id, incluyendo sus items
    /// </summary>
    Task<OrderEntity?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene todas las órdenes de un cliente
    /// </summary>
    Task<IEnumerable<OrderEntity>> GetByCustomerIdAsync(int customerId);

    /// <summary>
    /// Agrega una nueva orden a la base de datos
    /// </summary>
    Task<OrderEntity> AddAsync(OrderEntity order);

    /// <summary>
    /// Actualiza una orden existente
    /// </summary>
    Task<OrderEntity> UpdateAsync(OrderEntity order);

    /// <summary>
    /// Elimina una orden por su id
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Guarda los cambios en la base de datos
    /// </summary>
    Task SaveChangesAsync();
}
