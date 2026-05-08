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

// Configure the HTTP request pipeline
// Habilitamos Swagger siempre para facilitar las pruebas iniciales
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.Run();
