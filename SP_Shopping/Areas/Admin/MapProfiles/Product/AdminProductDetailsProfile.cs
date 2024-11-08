using AutoMapper;
using SP_Shopping.Areas.Admin.Dtos.Product;

namespace SP_Shopping.Areas.Admin.MapProfiles.Product;

public class AdminProductDetailsProfile : Profile
{
    public AdminProductDetailsProfile()
    {
        CreateMap<Models.Product, AdminProductDetailsDto>()
            .ForMember(p => p.SubmitterName, opt => opt.MapFrom(pp => pp.Submitter!.UserName))
            .ReverseMap();
    }
}
