namespace Order.Domain.Models.DTOs;

/// <summary>
/// DTO para crear una nueva orden
/// Contiene los productos que se desean comprar
/// </summary>
public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public List<OrderItemRequestDto> Items { get; set; } = new();
}

/// <summary>
/// DTO para representar un producto en la solicitud de crear orden
/// </summary>
public class OrderItemRequestDto
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int QuantityRequested { get; set; }
}
