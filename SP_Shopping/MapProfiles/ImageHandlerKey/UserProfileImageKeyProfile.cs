using AutoMapper;
using SP_Shopping.Models;
using SP_Shopping.Utilities.ImageHandlerKeys;

namespace SP_Shopping.MapProfiles.ImageHandlerKey;

public class UserProfileImageKeyProfile : Profile
{
    public UserProfileImageKeyProfile()
    {
        CreateMap<ApplicationUser, UserProfileImageKey>();
    }
}
