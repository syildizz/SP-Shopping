using AutoMapper;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Models;
using SP_Shopping.Dtos;

namespace SP_Shopping.MapProfiles;

public class ProductImageKeyProfile : Profile
{
    public ProductImageKeyProfile()
    {
        CreateMap<Product, ProductImageKey>();
        CreateMap<ProductCreateDto, ProductImageKey>();
        CreateMap<ProductDetailsDto, ProductImageKey>();
        CreateMap<UserPageDto.UserPageProductDto, ProductImageKey>();
    }
}
