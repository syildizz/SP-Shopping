using AutoMapper;
using SP_Shopping.Dtos;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles
{
    public class CartItemDetailsProfile : Profile
    {
        public CartItemDetailsProfile()
        {
            CreateMap<CartItem, CartItemDetailsDto>().ReverseMap();
        }

    }
}
