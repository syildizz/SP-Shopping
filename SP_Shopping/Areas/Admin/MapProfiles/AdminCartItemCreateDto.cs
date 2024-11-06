using AutoMapper;
using SP_Shopping.Areas.Admin.Dtos.Cart;
using SP_Shopping.Models;

namespace SP_Shopping.Areas.Admin.MapProfiles;

public class AdminCartItemCreateProfile : Profile
{
    public AdminCartItemCreateProfile()
    {
        CreateMap<CartItem, AdminCartItemCreateDto>().ReverseMap();
    }
}
