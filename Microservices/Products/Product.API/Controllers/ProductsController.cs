using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Product.Domain.Models.DTOs;
using Product.Domain.Repositories;

namespace Product.API.Controllers;

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
