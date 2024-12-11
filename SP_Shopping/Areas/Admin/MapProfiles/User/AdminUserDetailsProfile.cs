using AutoMapper;
using SP_Shopping.Areas.Admin.Dtos.User;
using SP_Shopping.Models;

namespace SP_Shopping.Areas.Admin.MapProfiles.User;

public class AdminUserDetailsProfile : Profile
{

    public AdminUserDetailsProfile()
    {
        CreateMap<ApplicationUser, AdminUserDetailsDto>()
            .ForMember(udto => udto.Roles, opt => opt.MapFrom(u => u.Roles.Select(r => r.Name ?? "")))
            //.ReverseMap()
            //.ForMember(u => u.Roles, opt => opt.MapFrom(udto => udto.Roles.Select(r => new ApplicationRole(r)).ToList()))
        ;
    }

}
