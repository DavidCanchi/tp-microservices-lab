using AutoMapper;
using Product.Domain.Models.DTOs;

namespace Product.Application.Mappings;

public class ProductMapping : Profile
{
    public ProductMapping()
    {
        CreateMap<global::Product.Domain.Models.Entities.Product, ProductDto>().ReverseMap();
    }
}
