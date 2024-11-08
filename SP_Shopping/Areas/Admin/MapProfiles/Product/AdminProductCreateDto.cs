using AutoMapper;
using SP_Shopping.Areas.Admin.Dtos.Product;

namespace SP_Shopping.Areas.Admin.MapProfiles.Product;

public class AdminProductCreateProfile : Profile
{
    public AdminProductCreateProfile()
    {
        CreateMap<Models.Product, AdminProductCreateDto>()
            .ReverseMap();
    }
}
