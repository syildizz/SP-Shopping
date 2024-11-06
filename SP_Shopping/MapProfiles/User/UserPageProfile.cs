using AutoMapper;
using SP_Shopping.Dtos.Product;
using SP_Shopping.Dtos.User;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles.User;

public class UserPageProfile : Profile
{
    public UserPageProfile()
    {
        CreateMap<ApplicationUser, UserPageDto>()
            .ForMember(u => u.ProductDetails, opt => opt.MapFrom(uu => uu.Products))
            .ReverseMap();

        CreateMap<Models.Product, UserPageDto.UserPageProductDto>()
            .ReverseMap();

        CreateMap<ProductDetailsDto, UserPageDto.UserPageProductDto>()
            .ReverseMap();

    }

}
