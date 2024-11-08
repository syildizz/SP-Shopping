using AutoMapper;
using SP_Shopping.Dtos.Cart;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles.Cart;

public class CartItemCreateProfile : Profile
{
    public CartItemCreateProfile()
    {
        CreateMap<CartItem, CartItemCreateDto>().ReverseMap();
    }
}
