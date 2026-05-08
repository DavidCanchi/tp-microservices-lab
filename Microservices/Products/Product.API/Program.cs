using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Product.Application.Mappings;
using Product.Domain.Repositories;
using Product.Infrastructure.Data;
using Product.Infrastructure.Repositories;
using Product.API.Validations;
using Product.Domain.Models.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar DbContext - In-Memory Database
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseInMemoryDatabase("ProductDb"));

// Registrar Repositorio
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Registrar AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(ProductMapping).Assembly);

var app = builder.Build();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new Product.Domain.Models.Entities.Product 
            { 
                Name = "Laptop Dell XPS 13", 
                Description = "Laptop ultradelgada con procesador Intel i7, 16GB RAM, 512GB SSD", 
                Price = 1200.00m, 
                Stock = 10 
            },
            new Product.Domain.Models.Entities.Product 
            { 
                Name = "Mouse Logitech MX Master 3", 
                Description = "Mouse inalámbrico de precisión para productividad", 
                Price = 99.99m, 
                Stock = 50 
            },
            new Product.Domain.Models.Entities.Product 
            { 
                Name = "Teclado Mecánico Corsair", 
                Description = "Teclado mecánico RGB con switches Cherry MX", 
                Price = 150.00m, 
                Stock = 25 
            },
            new Product.Domain.Models.Entities.Product 
            { 
                Name = "Monitor LG 27\" 4K", 
                Description = "Monitor 4K UltraFine con color accuracy profesional", 
                Price = 599.99m, 
                Stock = 5 
            },
            new Product.Domain.Models.Entities.Product 
            { 
                Name = "Hub USB-C Anker", 
                Description = "Hub con 7 puertos USB-C, HDMI, SD card reader", 
                Price = 79.99m, 
                Stock = 100 
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
