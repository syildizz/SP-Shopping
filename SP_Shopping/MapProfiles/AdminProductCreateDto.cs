using AutoMapper;
using SP_Shopping.Areas.Admin.Dtos;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles;

public class AdminProductCreateProfile : Profile
{
    public AdminProductCreateProfile()
    {
        CreateMap<Product, AdminProductCreateDto>()
            .ReverseMap();
    }
}
