using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Customer.Domain.Models.DTOs;
using Customer.Domain.Models.Entities;
using Customer.Domain.Repositories;

namespace Customer.API.Controllers;

//alias para evitar la ambigüedad con el namespace
using CustomerEntity = global::Customer.Domain.Models.Entities.Customer;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper _mapper;

    public CustomersController(ICustomerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
    {
        var customers = await _repository.GetAllAsync();
        var customersDto = _mapper.Map<IEnumerable<CustomerDto>>(customers);
        return Ok(customersDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer == null)
            return NotFound($"Cliente con id {id} no encontrado");

        var customerDto = _mapper.Map<CustomerDto>(customer);
        return Ok(customerDto);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CustomerDto customerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = _mapper.Map<CustomerEntity>(customerDto);
        customer.RegistrationDate = DateTime.Now;
        
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, _mapper.Map<CustomerDto>(customer));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerDto customerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = await _repository.GetByIdAsync(id);
        if (customer == null)
            return NotFound($"Cliente con id {id} no encontrado");

        customer.Name = customerDto.Name;
        customer.Email = customerDto.Email;
        customer.Address = customerDto.Address;

        await _repository.UpdateAsync(customer);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            return NotFound($"Cliente con id {id} no encontrado");

        await _repository.SaveChangesAsync();
        return NoContent();
    }
}
