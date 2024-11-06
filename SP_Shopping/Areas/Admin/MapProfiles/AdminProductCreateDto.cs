using AutoMapper;
using SP_Shopping.Areas.Admin.Dtos.Product;
using SP_Shopping.Models;

namespace SP_Shopping.Areas.Admin.MapProfiles;

public class AdminProductCreateProfile : Profile
{
    public AdminProductCreateProfile()
    {
        CreateMap<Product, AdminProductCreateDto>()
            .ReverseMap();
    }
}
