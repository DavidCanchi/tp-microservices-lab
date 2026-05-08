using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Product.Domain.Models.DTOs;
using Product.Domain.Repositories;

namespace Product.API.Controllers;

/// <summary>
/// Request para actualizar el stock de un producto
/// </summary>
public class StockUpdateRequest
{
    /// <summary>
    /// Cantidad a sumar o restar al stock (negativo para reducir)
    /// </summary>
    public int Stock { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public ProductsController(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _repository.GetAllAsync();
        var productsDto = _mapper.Map<IEnumerable<ProductDto>>(products);
        return Ok(productsDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return NotFound($"Producto con id {id} no encontrado");

        var productDto = _mapper.Map<ProductDto>(product);
        return Ok(productDto);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductDto productDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = _mapper.Map<global::Product.Domain.Models.Entities.Product>(productDto);
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, _mapper.Map<ProductDto>(product));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductDto productDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return NotFound($"Producto con id {id} no encontrado");

        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Price = productDto.Price;
        product.Stock = productDto.Stock;

        await _repository.UpdateAsync(product);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Actualiza el stock de un producto (reducir o aumentar)
    /// Body: { "stock": -5 } para reducir 5 unidades, o { "stock": 10 } para aumentar 10
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] StockUpdateRequest request)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return NotFound($"Producto con id {id} no encontrado");

        int newStock = product.Stock + request.Stock;
        if (newStock < 0)
            return BadRequest($"Stock no puede ser negativo. Stock actual: {product.Stock}, cambio: {request.Stock}");

        product.Stock = newStock;
        await _repository.UpdateAsync(product);
        await _repository.SaveChangesAsync();

        return Ok(new { id = product.Id, name = product.Name, newStock = product.Stock });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            return NotFound($"Producto con id {id} no encontrado");

        await _repository.SaveChangesAsync();
        return NoContent();
    }
}
