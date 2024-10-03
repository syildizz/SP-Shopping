using AutoMapper;
using SP_Shopping.Dtos;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles;

public class CartItemCreateProfile : Profile
{
    public CartItemCreateProfile()
    {
        CreateMap<CartItem, CartItemCreateDto>().ReverseMap();
    }
}
