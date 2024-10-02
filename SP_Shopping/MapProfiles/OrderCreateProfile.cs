using AutoMapper;
using SP_Shopping.Dtos;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles
{
    public class OrderCreateProfile : Profile
    {
        public OrderCreateProfile()
        {
            CreateMap<Order, OrderCreateDto>()
                .ForMember(ocdto => ocdto.UserName, opt => opt.MapFrom(order => order.User.UserName))
                .ForMember(ocdto => ocdto.ProductNames, opt => opt.MapFrom(order => order.Products.Select(p => p.Name).ToList()))
                .ReverseMap()
                //.ForMember(order => order.User.UserName, o => o.MapFrom(ocdto => ocdto.UserName))
                //.ForMember(order => order.Products, o => o.MapFrom(ocdto => ocdto.UserName))
                ;

        }
    }
}
