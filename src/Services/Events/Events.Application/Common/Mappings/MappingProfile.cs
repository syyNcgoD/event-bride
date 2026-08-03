using AutoMapper;
using Events.Application.DTOs;
using Events.Domain.Entities;

namespace Events.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Event, EventSummaryResponse>()
            .ForMember(dest => dest.VenueName, opt => opt.MapFrom(src => src.Venue!.Name))
            .ForMember(dest => dest.VenueCity, opt => opt.MapFrom(src => src.Venue!.City))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category!.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.MinTicketPrice,
                opt => opt.MapFrom(src => src.TicketTypes.Count > 0 ? src.TicketTypes.Min(tt => tt.Price) : 0))
            .ForMember(dest => dest.AvailableTickets,
                opt => opt.MapFrom(src => src.TicketTypes.Sum(tt => tt.AvailableQuantity)));
    }
}
