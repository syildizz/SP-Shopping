using AutoMapper;
using SP_Shopping.Dtos.Product;

namespace SP_Shopping.MapProfiles.Product;

public class ProductCreateProfile : Profile
{
    public ProductCreateProfile()
    {
        CreateMap<Models.Product, ProductCreateDto>()
            .ReverseMap();
    }
}
