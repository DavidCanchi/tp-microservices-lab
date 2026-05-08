using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Customer.Application.Mappings;
using Customer.Domain.Repositories;
using Customer.Infrastructure.Data;
using Customer.Infrastructure.Repositories;
using Customer.API.Validations;
using Customer.Domain.Models.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar DbContext - In-Memory Database
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseInMemoryDatabase("CustomerDb"));

// Registrar Repositorio
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Registrar AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(CustomerMapping).Assembly);

var app = builder.Build();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    if (!context.Customers.Any())
    {
        context.Customers.AddRange(
            new Customer.Domain.Models.Entities.Customer
            {
                Name = "Carlos López",
                Email = "carlos.lopez@email.com",
                Address = "Calle Principal 123, Ciudad",
                RegistrationDate = DateTime.Now.AddDays(-30)
            },
            new Customer.Domain.Models.Entities.Customer
            {
                Name = "Juan García",
                Email = "juan.garcia@email.com",
                Address = "Avenida Central 456, Ciudad",
                RegistrationDate = DateTime.Now.AddDays(-20)
            },
            new Customer.Domain.Models.Entities.Customer
            {
                Name = "María Rodríguez",
                Email = "maria.rodriguez@email.com",
                Address = "Plaza Mayor 789, Ciudad",
                RegistrationDate = DateTime.Now.AddDays(-10)
            },
            new Customer.Domain.Models.Entities.Customer
            {
                Name = "Ana Martínez",
                Email = "ana.martinez@email.com",
                Address = "Paseo del Parque 321, Ciudad",
                RegistrationDate = DateTime.Now.AddDays(-5)
            }
        );
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline
// Habilitamos Swagger siempre para facilitar las pruebas iniciales
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

app.UseRouting();
app.MapControllers();

app.Run();
