using AutoMapper;
using SP_Shopping.Dtos.Cart;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles.Cart;

public class CartItemDetailsProfile : Profile
{
    public CartItemDetailsProfile()
    {
        CreateMap<CartItem, CartItemDetailsDto>()
            .ForMember(c => c.UserName, opt => opt.MapFrom(c => c.User.UserName))
            .ForMember(c => c.SubmitterName, opt => opt.MapFrom(c => c.Product.Submitter == null ? null : c.Product.Submitter.UserName))
            .ForMember(c => c.Price, opt => opt.MapFrom(c => c.Product.Price))
            .ReverseMap();
    }

}
