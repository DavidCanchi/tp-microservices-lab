using System.Diagnostics.CodeAnalysis;

namespace Order.Domain.Models.Entities;

public class Order
{
    public int Id { get; set; }
    public required DateTime OrderDate { get; set; }
    public int CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Colección de items de la orden
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public Order() { }

    [SetsRequiredMembers]
    public Order(int customerId, string customerName)
    {
        CustomerId = customerId;
        CustomerName = customerName;
        OrderDate = DateTime.Now;
        TotalAmount = 0;
    }

    /// <summary>
    /// Agrega un item a la orden y calcula el subtotal
    /// </summary>
    public void AddOrderItem(int productId, string productName, decimal unitPrice, int quantity)
    {
        var orderItem = new OrderItem
        {
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Subtotal = unitPrice * quantity
        };

        OrderItems.Add(orderItem);
        RecalculateTotalAmount();
    }

    /// <summary>
    /// Recalcula el monto total de la orden
    /// </summary>
    public void RecalculateTotalAmount()
    {
        TotalAmount = OrderItems.Sum(item => item.Subtotal);
    }
}
