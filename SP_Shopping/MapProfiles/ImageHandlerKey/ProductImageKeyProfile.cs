using AutoMapper;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Models;
using SP_Shopping.Dtos.Product;
using SP_Shopping.Dtos.User;

namespace SP_Shopping.MapProfiles.ImageHandlerKey;

public class ProductImageKeyProfile : Profile
{
    public ProductImageKeyProfile()
    {
        CreateMap<Models.Product, ProductImageKey>();
        CreateMap<ProductCreateDto, ProductImageKey>();
        CreateMap<ProductDetailsDto, ProductImageKey>();
        CreateMap<UserPageDto.UserPageProductDto, ProductImageKey>();
    }
}
