using AutoMapper;
using SP_Shopping.Dtos;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles;

public class UserPageProfile : Profile
{
    public UserPageProfile()
    {
        CreateMap<ApplicationUser, UserPageDto>()
            .ForMember(u => u.ProductDetails, opt => opt.MapFrom(uu => uu.Products))
            .ReverseMap();

        CreateMap<Product, UserPageDto.UserPageProductDto>()
            .ReverseMap();
    }
        
}
