using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SP_Shopping.Areas.Admin.Dtos.User;
using SP_Shopping.Data;
using SP_Shopping.Models;

namespace SP_Shopping.Areas.Admin.MapProfiles.User;

public class AdminUserEditProfile : Profile
{

    public AdminUserEditProfile()
    {
        CreateMap<ApplicationUser, AdminUserEditDto>()
            .ForMember(udto => udto.Roles, opt => opt.MapFrom(u => u.Roles.Where(r => r.Name != null).Select(r => r.Name)))
            .ForMember(udto => udto.RoleString, opt => opt.Ignore())
            .ForMember(udto => udto.ProfilePicture, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(u => u.Roles, opt => opt.MapFrom(udto => new List<ApplicationRole>()))
        ;
    }
}
