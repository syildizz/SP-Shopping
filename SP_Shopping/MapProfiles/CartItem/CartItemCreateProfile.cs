using AutoMapper;
using SP_Shopping.Dtos.Cart;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles.CartItem;

public class CartItemCreateProfile : Profile
{
    public CartItemCreateProfile()
    {
        CreateMap<Models.CartItem, CartItemCreateDto>().ReverseMap();
    }
}
