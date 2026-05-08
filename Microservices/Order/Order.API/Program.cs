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

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var orderDbName = builder.Configuration["Database:OrderDbName"] ?? "OrderDb";
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase(orderDbName));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddAutoMapper(cfg => { }, typeof(OrderMapping).Assembly);

builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
{
    var productApiUrl = builder.Configuration["Services:ProductApi"];
    if (string.IsNullOrEmpty(productApiUrl))
        throw new InvalidOperationException("Services:ProductApi debe estar configurado en appsettings.json");
    
    client.BaseAddress = new Uri(productApiUrl);
    client.Timeout = TimeSpan.FromSeconds(double.Parse(
        builder.Configuration["Services:RequestTimeoutSeconds"] ?? "10"));
});

builder.Services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
{
    var customerApiUrl = builder.Configuration["Services:CustomerApi"];
    if (string.IsNullOrEmpty(customerApiUrl))
        throw new InvalidOperationException("Services:CustomerApi debe estar configurado en appsettings.json");
    
    client.BaseAddress = new Uri(customerApiUrl);
    client.Timeout = TimeSpan.FromSeconds(double.Parse(
        builder.Configuration["Services:RequestTimeoutSeconds"] ?? "10"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
// app.UseHttpsRedirection();

app.UseRouting();
app.MapControllers();

app.Run();
