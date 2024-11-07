using AutoMapper;
using SP_Shopping.Dtos.Cart;

namespace SP_Shopping.MapProfiles.CartItem;

public class AdminCartItemDetailsProfile : Profile
{
    public AdminCartItemDetailsProfile()
    {
        CreateMap<Models.CartItem, AdminCartItemDetailsDto>()
            .ForMember(c => c.UserName, opt => opt.MapFrom(c => c.User.UserName))
            .ForMember(c => c.SubmitterName, opt => opt.MapFrom(c => c.Product.Submitter == null ? null : c.Product.Submitter.UserName))
            .ForMember(c => c.SubmitterId, opt => opt.MapFrom(c => c.Product.SubmitterId == null ? null : c.Product.Submitter.Id))
            .ReverseMap();
    }

}
