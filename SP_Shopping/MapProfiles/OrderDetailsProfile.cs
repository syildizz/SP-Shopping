using AutoMapper;
using SP_Shopping.Dtos;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles
{
    public class OrderDetailsProfile : Profile
    {
        public OrderDetailsProfile()
        {
            CreateMap<Order, OrderDetailsDto>()
                .ForMember(oddto => oddto.UserName, opt => opt.MapFrom(order => order.User.UserName))
                .ForMember(oddto => oddto.ProductNames, opt => opt.MapFrom(order => order.Products.Select(p => p.Name).ToList()))
                .ReverseMap()
           ;
        }
    }
}
