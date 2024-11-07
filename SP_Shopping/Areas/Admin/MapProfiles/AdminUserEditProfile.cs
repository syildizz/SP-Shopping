using AutoMapper;
using SP_Shopping.Areas.Admin.Dtos.User;
using SP_Shopping.Models;

namespace SP_Shopping.Areas.Admin.MapProfiles;

public class AdminUserEditProfile : Profile
{

    public AdminUserEditProfile()
    {
        CreateMap<ApplicationUser, AdminUserEditDto>()
            .ReverseMap()
        ;
    }
}
