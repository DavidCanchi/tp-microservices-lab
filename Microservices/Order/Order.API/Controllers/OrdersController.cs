using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Order.Domain.Models.DTOs;
using Order.Domain.Repositories;

namespace Order.API.Controllers;

using OrderEntity = global::Order.Domain.Models.Entities.Order;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    // En un escenario real, estos servicios vendrían del HttpClientFactory o un cliente de API
    // Para esta demostración, asumimos que existen servicios externos que podríamos llamar

    public OrdersController(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
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
    /// 1. Valida que el cliente y productos existan
    /// 2. Verifica el stock disponible de cada producto
    /// 3. Si la cantidad pedida > stock disponible, permite comprar solo la cantidad disponible
    /// 4. Calcula el precio total de la orden
    /// 5. Crea la orden con los items ajustados según disponibilidad
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto createOrderDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (createOrderDto.Items == null || createOrderDto.Items.Count == 0)
            return BadRequest("La orden debe contener al menos un producto");

        try
        {
            // Crear la orden
            var order = new OrderEntity(createOrderDto.CustomerId, createOrderDto.CustomerName);

            // Procesar cada item de la orden
            foreach (var item in createOrderDto.Items)
            {
                // En una aplicación real, aquí se llamaría al microservicio de Productos para:
                // 1. Verificar que el producto existe
                // 2. Obtener el stock disponible
                // 3. Actualizar el stock

                // Para este ejemplo, asumimos que el stock está disponible en la cantidad solicitada
                // IMPORTANTE: En producción, esto sería:
                // var product = await productService.GetProductAsync(item.ProductId);
                // if (product == null) return BadRequest($"Producto {item.ProductId} no encontrado");
                // 
                // var quantityToAdd = Math.Min(item.QuantityRequested, product.Stock);
                // if (quantityToAdd == 0)
                //     return BadRequest($"Producto {item.ProductName} sin stock disponible");

                var quantityToAdd = item.QuantityRequested; // En este ejemplo, asumimos disponibilidad completa

                // Agregar el item a la orden
                order.AddOrderItem(
                    item.ProductId,
                    item.ProductName,
                    item.UnitPrice,
                    quantityToAdd
                );
            }

            // Guardar la orden
            await _repository.AddAsync(order);
            await _repository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, _mapper.Map<OrderDto>(order));
        }
        catch (Exception ex)
        {
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
