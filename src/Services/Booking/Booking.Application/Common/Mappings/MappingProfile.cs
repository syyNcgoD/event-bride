using AutoMapper;
using Booking.Application.DTOs;
using Booking.Domain.Entities;

namespace Booking.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments));

        CreateMap<OrderItem, OrderItemResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        CreateMap<Payment, PaymentResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}