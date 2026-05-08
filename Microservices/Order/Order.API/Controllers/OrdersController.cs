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

    /// <summary>
    /// Obtiene todas las órdenes con su historial completo
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await _repository.GetAllAsync();
        var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
        return Ok(ordersDto);
    }

    /// <summary>
    /// Obtiene una orden específica por su id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
            return NotFound($"Orden con id {id} no encontrada");

        var orderDto = _mapper.Map<OrderDto>(order);
        return Ok(orderDto);
    }

    /// <summary>
    /// Obtiene el historial de órdenes de un cliente específico
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCustomerId(int customerId)
    {
        var orders = await _repository.GetByCustomerIdAsync(customerId);
        var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
        return Ok(ordersDto);
    }

    /// <summary>
    /// Crea una nueva orden
    /// 
    /// LÓGICA DE NEGOCIO:
    /// 1. Valida que el cliente exista en Customer.API
    /// 2. Verifica el stock disponible de cada producto en Product.API
    /// 3. Si la cantidad pedida > stock disponible, permite comprar solo la cantidad disponible
    /// 4. Actualiza el stock de los productos en Product.API
    /// 5. Calcula el precio total de la orden
    /// 6. Crea la orden con los items ajustados según disponibilidad
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto createOrderDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // 1. Validar que el cliente existe
            _logger.LogInformation($"Validando cliente {createOrderDto.CustomerId}");
            var customer = await _customerServiceClient.GetCustomerAsync(createOrderDto.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning($"Cliente {createOrderDto.CustomerId} no encontrado");
                return BadRequest($"Cliente con ID {createOrderDto.CustomerId} no existe en el sistema");
            }

            // 2. Crear la orden
            var order = new OrderEntity(createOrderDto.CustomerId, createOrderDto.CustomerName);

            // 3. Procesar cada item y validar stock
            decimal totalOrderAmount = 0;
            var processedItems = new List<(int ProductId, string ProductName, decimal UnitPrice, int Quantity)>();

            foreach (var item in createOrderDto.Items)
            {
                _logger.LogInformation($"Validando disponibilidad del producto {item.ProductId}");
                
                // Obtener información del producto
                var product = await _productServiceClient.GetProductAsync(item.ProductId);
                if (product == null)
                {
                    _logger.LogWarning($"Producto {item.ProductId} no encontrado");
                    return BadRequest($"Producto con ID {item.ProductId} no existe en el sistema");
                }

                // Calcular cantidad disponible
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

                // Registrar el item procesado
                processedItems.Add((item.ProductId, product.Name, product.Price, quantityToAdd));
                totalOrderAmount += product.Price * quantityToAdd;
            }

            // 4. Si todo validó correctamente, actualizar stock y crear items de orden
            foreach (var (productId, productName, unitPrice, quantity) in processedItems)
            {
                _logger.LogInformation($"Actualizando stock del producto {productId}");
                
                // Actualizar stock en Product.API
                var stockUpdated = await _productServiceClient.UpdateProductStockAsync(productId, quantity);
                if (!stockUpdated)
                {
                    _logger.LogError($"Error al actualizar stock del producto {productId}");
                    // En un sistema real, podrían hacer rollback aquí
                    return StatusCode(500, $"Error al actualizar stock del producto {productName}");
                }

                // Agregar el item a la orden
                order.AddOrderItem(productId, productName, unitPrice, quantity);
            }

            // 5. Guardar la orden
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

    /// <summary>
    /// Actualiza una orden existente
    /// NOTA: Típicamente las órdenes una vez creadas no se pueden modificar.
    /// Esta operación estaría disponible solo en estados específicos (ej: "Pendiente").
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] OrderDto orderDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var order = await _repository.GetByIdAsync(id);
        if (order == null)
            return NotFound($"Orden con id {id} no encontrada");

        // En una aplicación real, validaríamos el estado de la orden
        // y solo permitiríamos actualizaciones en estados específicos

        order.CustomerName = orderDto.CustomerName;

        await _repository.UpdateAsync(order);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Cancela una orden (eliminación lógica o cambio de estado)
    /// En un sistema real, generalmente no se elimina una orden, sino se marca como "Cancelada"
    /// </summary>
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
