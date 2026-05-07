using AutoMapper;
using Order.Domain.Models.DTOs;
using Order.Domain.Models.Entities;

namespace Order.Application.Mappings;

public class OrderMapping : Profile
{
    public OrderMapping()
    {
        // Mapear Order a OrderDto
        CreateMap<global::Order.Domain.Models.Entities.Order, OrderDto>()
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
            .ReverseMap();

        // Mapear OrderItem a OrderItemDto
        CreateMap<global::Order.Domain.Models.Entities.OrderItem, OrderItemDto>().ReverseMap();
    }
}
