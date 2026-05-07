using AutoMapper;
using Customer.Domain.Models.DTOs;

namespace Customer.Application.Mappings;

public class CustomerMapping : Profile
{
    public CustomerMapping()
    {
        CreateMap<global::Customer.Domain.Models.Entities.Customer, CustomerDto>().ReverseMap();
    }
}
