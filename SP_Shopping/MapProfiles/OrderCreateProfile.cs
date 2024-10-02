using AutoMapper;
using SP_Shopping.Dtos;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles
{
    public class OrderCreateProfile : Profile
    {
        public OrderCreateProfile()
        {
            CreateMap<Order, OrderCreateDto>().ReverseMap();
        }
    }
}
