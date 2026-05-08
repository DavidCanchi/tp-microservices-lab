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

// Configure the HTTP request pipeline
// Habilitamos Swagger siempre para facilitar las pruebas iniciales
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.Run();
