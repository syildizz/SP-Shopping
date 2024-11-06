using AutoMapper;
using SP_Shopping.Areas.Admin.Dtos.User;
using SP_Shopping.Models;

namespace SP_Shopping.Areas.Admin.MapProfiles;

public class AdminUserDetailsProfile : Profile
{

    public AdminUserDetailsProfile()
    {
        CreateMap<ApplicationUser, AdminUserDetailsDto>()
            .ReverseMap()
        ;
    }
}
