using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Order.Domain.Models.DTOs;
using Order.Domain.Repositories;
using Order.API.Services;

namespace Order.API.Controllers;

using OrderEntity = global::Order.Domain.Models.Entities.Order;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly IProductServiceClient _productServiceClient;
    private readonly ICustomerServiceClient _customerServiceClient;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderRepository repository, 
        IMapper mapper,
        IProductServiceClient productServiceClient,
        ICustomerServiceClient customerServiceClient,
        ILogger<OrdersController> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _productServiceClient = productServiceClient;
        _customerServiceClient = customerServiceClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await _repository.GetAllAsync();
        var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
        return Ok(ordersDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
            return NotFound($"Orden con id {id} no encontrada");

        var orderDto = _mapper.Map<OrderDto>(order);
        return Ok(orderDto);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCustomerId(int customerId)
    {
        var orders = await _repository.GetByCustomerIdAsync(customerId);
        var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
        return Ok(ordersDto);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto createOrderDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var customer = await _customerServiceClient.GetCustomerAsync(createOrderDto.CustomerId);
            
            if (customer == null)
            {
                _logger.LogWarning($"Cliente {createOrderDto.CustomerId} no encontrado");
                return BadRequest($"Cliente con ID {createOrderDto.CustomerId} no existe en el sistema");
            }
            
            var order = new OrderEntity(createOrderDto.CustomerId, createOrderDto.CustomerName);
            var processedItems = new List<(int ProductId, string ProductName, decimal UnitPrice, int Quantity)>();
            foreach (var item in createOrderDto.Items)
            {
                var product = await _productServiceClient.GetProductAsync(item.ProductId);
                if (product == null)
                {
                    _logger.LogWarning($"Producto {item.ProductId} no encontrado");
                    return BadRequest($"Producto con ID {item.ProductId} no existe en el sistema");
                }

                int quantityToAdd = Math.Min(item.QuantityRequested, product.Stock);
                
                if (quantityToAdd == 0)
                {
                    _logger.LogWarning($"Producto {product.Name} sin stock disponible");
                    return BadRequest($"Producto '{product.Name}' no tiene stock disponible");
                }

                if (quantityToAdd < item.QuantityRequested)
                {
                    _logger.LogInformation($"Stock limitado para {product.Name}: se comprarán {quantityToAdd} de {item.QuantityRequested} solicitados");
                }

                processedItems.Add((item.ProductId, product.Name, product.Price, quantityToAdd));
            }

            foreach (var (productId, productName, unitPrice, quantity) in processedItems)
            {
                var stockUpdated = await _productServiceClient.UpdateProductStockAsync(productId, quantity);
                if (!stockUpdated)
                {
                    _logger.LogError($"Error al actualizar stock del producto {productId}");
                    return StatusCode(500, $"Error al actualizar stock del producto {productName}");
                }
                order.AddOrderItem(productId, productName, unitPrice, quantity);
            }
            await _repository.AddAsync(order);
            await _repository.SaveChangesAsync();

            _logger.LogInformation($"Orden {order.Id} creada exitosamente");
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, _mapper.Map<OrderDto>(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la orden");
            return StatusCode(500, $"Error al crear la orden: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] OrderDto orderDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var order = await _repository.GetByIdAsync(id);
        if (order == null)
            return NotFound($"Orden con id {id} no encontrada");

        order.CustomerName = orderDto.CustomerName;

        await _repository.UpdateAsync(order);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            return NotFound($"Orden con id {id} no encontrada");

        await _repository.SaveChangesAsync();
        return NoContent();
    }
}
