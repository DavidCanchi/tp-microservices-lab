using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Order.Application.Mappings;
using Order.Domain.Repositories;
using Order.Infrastructure.Data;
using Order.Infrastructure.Repositories;
using Order.API.Validations;
using Order.API.Services;
using Order.Domain.Models.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar DbContext - In-Memory Database
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrderDb"));

// Registrar Repositorio
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Registrar AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(OrderMapping).Assembly);

// Registrar HttpClient para integraciones con otros microservicios
builder.Services.AddHttpClient<ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductApi"] ?? "http://localhost:5001");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<CustomerServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CustomerApi"] ?? "http://localhost:5000");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<IProductServiceClient, ProductServiceClient>();
builder.Services.AddScoped<ICustomerServiceClient, CustomerServiceClient>();

var app = builder.Build();

// Configure the HTTP request pipeline
// Habilitamos Swagger siempre para facilitar las pruebas iniciales
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.Run();
